using CEG.Extensions;
using CEG.Gameplay.Shared;
using CEG.Gameplay.Shared.Components;
using System;
using System.Net;
using System.Net.Sockets;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Networking.Transport;
using Unity.Transforms;

namespace CEG.Gameplay.Server.Systems
{
    [UpdateBefore(typeof(GhostSendSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class ServerSystem : SystemBase
    {
        private static ComponentLookup<NetworkId> _networkIdFromEntity;
        private NetworkStreamRequestListenResult.State _listenState;

        private Entity _playerPrefab;

        protected override void OnCreate()
        {
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnCreate()");
            base.OnCreate();

            RequireForUpdate<SystemsSettings>();

            _networkIdFromEntity = GetComponentLookup<NetworkId>(true);
        }

        protected override void OnStartRunning()
        {
            Log.Verbose("OnStartRunning");
            base.OnStartRunning();

            _playerPrefab = SystemAPI.GetSingleton<PlayerPrefab>().Value;

            Listen();
        }

        protected override void OnStopRunning()
        {
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnStopRunning()");
            base.OnStopRunning();
        }

        protected override void OnDestroy()
        {
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnDestroy()");
            base.OnDestroy();
        }
        
        protected override void OnUpdate()
        {
            _networkIdFromEntity.Update(this);

            GetListeningStatus();
            RPC_Connect();
        }

        private void Listen()
        {
            if (SystemAPI.TryGetSingleton<SystemsSettings>(out var ss))
            {
                Log.Debug($"SystemsSettings: {ss.Fqdn}:{ss.Port}");

                IPAddress[] ipv4Addresses = Array.FindAll(Dns.GetHostEntry(ss.Fqdn.ToString()).AddressList, a => a.AddressFamily == AddressFamily.InterNetwork);
                if (ipv4Addresses.Length > 0)
                {
                    Log.Debug($"ipv4Addresses[0]: {ipv4Addresses[0]}");

                    var endpoint = NetworkEndpoint.Parse("0.0.0.0", ss.Port);
                    Log.Debug($"endpoint (Dns.GetHostEntry): {endpoint}");

                    var ecb = new EntityCommandBuffer(WorldUpdateAllocator);
                    var req = ecb.CreateEntity();
                    ecb.AddComponent(req, new NetworkStreamRequestListen
                    {
                        Endpoint = endpoint
                    });
                    ecb.AddComponent<NetworkStreamRequestListenResult>(req);
                    ecb.Playback(EntityManager);
                    ecb.Dispose();
                }
                else
                {
                    Log.Fatal($"Dns.GetHostEntry could not resolve name {ss.Fqdn} to any valid ipv4");
                }
            }
            else
            {
                Log.Fatal($"SystemsSettings Singleton not present!!!");
            }

            foreach (var networkStreamRequestListenResult in SystemAPI.Query<RefRO<NetworkStreamRequestListenResult>>())
            {
                _listenState = networkStreamRequestListenResult.ValueRO.RequestState;
                Log.Debug($"NetworkStreamRequestListenResult: {_listenState}");

            }
        }

        private void GetListeningStatus()
        {
            foreach (var networkStreamRequestListenResult in SystemAPI.Query<RefRO<NetworkStreamRequestListenResult>>())
            {
                if (_listenState != networkStreamRequestListenResult.ValueRO.RequestState)
                {
                    _listenState = networkStreamRequestListenResult.ValueRO.RequestState;
                    Log.Info($"NetworkStreamRequestListenResult: {_listenState}");
                }
            }
        }

        private void RPC_Connect()
        {
            EntityManager.GetName(_playerPrefab, out var prefabName);
            var ecb = new EntityCommandBuffer(WorldUpdateAllocator);

            var networkTime = SystemAPI.GetSingleton<NetworkTime>();

            //EntitiesJournaling.Enabled = true;
            foreach (var (rpcSource, rpcEntity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>>()
                         .WithAll<RpcStartStreaming>()
                         .WithEntityAccess())
            {
                Log.Debug($"Processing RpcConnect for {rpcSource.ValueRO.SourceConnection}");
                
                var networkId = _networkIdFromEntity[rpcSource.ValueRO.SourceConnection].Value;
                
                Log.Debug($"Adding NetworkStreamInGame for NetworkId: {networkId}");
                ecb.AddComponent<NetworkStreamInGame>(rpcSource.ValueRO.SourceConnection);

                Log.Debug($"Instantiating prefab: {prefabName}");
                var player = EntityManager.Instantiate(_playerPrefab);
                
                EntityManager.SetComponentData(player, new GhostOwner
                {
                    NetworkId = networkId
                });

                //TODO: Grab Data From Database
                EntityManager.SetComponentData(player, new PlayerName
                {
                    Value = (FixedString64Bytes)$"Player {networkId}",
                });

                var localTransform = EntityManager.GetComponentData<LocalTransform>(_playerPrefab);
                EntityManager.SetComponentData(player, new LocalTransform
                {
                    Position = localTransform.Position,
                    Rotation = localTransform.Rotation,
                    Scale = localTransform.Scale,
                });
                
                var movementData = EntityManager.GetComponentData<MovementData>(_playerPrefab);
                EntityManager.SetComponentData(player, new MovementData
                {
                    Target = localTransform.Position,
                    Previous = localTransform.Position,
                    IsMoving = false,
                    ShouldCheckCollision = false,
                    PercentNextMove = movementData.PercentNextMove,
                });
                
                EntityManager.SetComponentData(player, new Reckoning
                {
                    Tick = networkTime.ServerTick,
                    ShouldReckon = true,
                    Target = localTransform.Position,
                });

                var healthPoints = EntityManager.GetComponentData<HealthPoints>(_playerPrefab);
                EntityManager.SetComponentData(player, new HealthPoints
                {
                    Value = healthPoints.MaxValue,
                    MaxValue = healthPoints.MaxValue
                });
                
                var damagePoints = EntityManager.GetComponentData<DamagePoints>(_playerPrefab);
                EntityManager.SetComponentData(player, new DamagePoints
                {
                    Value = damagePoints.Value
                });

                var color = new float4(
                    UnityEngine.Random.Range(0.0f, 1.0f),
                    UnityEngine.Random.Range(0.0f, 1.0f),
                    UnityEngine.Random.Range(0.0f, 1.0f),
                    1.0f);
                foreach (var linkedEntityGroup in EntityManager.GetBuffer<LinkedEntityGroup>(player))
                {
                    if (EntityManager.HasComponent<MaterialPropertyBaseColor>(linkedEntityGroup.Value))
                    {
                        EntityManager.SetComponentData(linkedEntityGroup.Value, new MaterialPropertyBaseColor
                        {
                            Value = color
                        });
                    }
                    
                    if (EntityManager.HasComponent<ColorAlive>(linkedEntityGroup.Value))
                    {
                        EntityManager.SetComponentData(linkedEntityGroup.Value, new ColorAlive
                        {
                            Value = color.ToColor()
                        });
                    }
                }
                
                // Add the player to the linked entity group so it is destroyed automatically on disconnect
                //ecb.AppendToBuffer(rpcSource.ValueRO.SourceConnection, new LinkedEntityGroup { Value = player });
                EntityManager.GetBuffer<LinkedEntityGroup>(rpcSource.ValueRO.SourceConnection).Add(player);

                ecb.DestroyEntity(rpcEntity);
            }

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}
