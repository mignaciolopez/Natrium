using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Natrium.Shared;
using Natrium.Gameplay.Shared;
using Natrium.Gameplay.Shared.Components;
using Natrium.Gameplay.Server.Components;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Networking.Transport;
using System.Net;
using System;
using System.Net.Sockets;

namespace Natrium.Gameplay.Server.Systems
{
    [UpdateInGroup(typeof(GhostSimulationSystemGroup), OrderLast = true)]
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
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnStartRunning()");
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

            //TODO: delete changemovement(), Resurrect()
            //changemovement();
            Resurrect();
        }

        //TODO: delete changemovement()
        private float elapsedtime = 0;
        private int currentMT = -1;
        private float interval = 7.5f;
        private void changemovement()
        {
            elapsedtime += SystemAPI.Time.DeltaTime;

            if (elapsedtime >= interval)
            {
                elapsedtime -= interval;

                foreach(var mt in SystemAPI.Query<RefRW<MovementType>>())
                {
                    currentMT++;
                    if (currentMT > 2)
                        currentMT = 0;

                    mt.ValueRW.Value = (MovementTypeEnum)currentMT;
                }
            }
        }

        private void Resurrect()
        {
            var ecb = new EntityCommandBuffer(WorldUpdateAllocator);
            const float range = 2.0f;
            
            foreach (var (ltw, dt, e) in SystemAPI.Query<LocalToWorld, EnabledRefRO<DeathTag>>()
                         .WithDisabled<ResurrectTag>().WithEntityAccess())
            {
                if (ltw.Position.x > -range && ltw.Position.x < range)
                {
                    if (ltw.Position.z > -range && ltw.Position.z < range)
                    {
                        Log.Debug($"Setting ResurrectTag to true on {e}");
                        ecb.SetComponentEnabled<ResurrectTag>(e, true);
                    }
                }
            }
            
            ecb.Playback(EntityManager);
            ecb.Dispose();
        }

        private void Listen()
        {
            if (SystemAPI.TryGetSingleton<SystemsSettings>(out var ss))
            {
                Log.Debug($"SystemsSettings: {ss.FQDN}:{ss.Port}");

                IPAddress[] ipv4Addresses = Array.FindAll(Dns.GetHostEntry(ss.FQDN.ToString()).AddressList, a => a.AddressFamily == AddressFamily.InterNetwork);
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
                    Log.Fatal($"Dns.GetHostEntry could not resolve name {ss.FQDN} to any valid ipv4");
                }
            }
            else
            {
                Log.Fatal($"SystemsSettings Singleton not present!!!");
            }

            foreach (var nsrlr in SystemAPI.Query<NetworkStreamRequestListenResult>())
            {
                _listenState = nsrlr.RequestState;
                Log.Debug($"NetworkStreamRequestListenResult: {_listenState}");

            }
        }

        private void GetListeningStatus()
        {
            foreach (var nsrlr in SystemAPI.Query<NetworkStreamRequestListenResult>())
            {
                if (_listenState != nsrlr.RequestState)
                {
                    _listenState = nsrlr.RequestState;
                    Log.Info($"NetworkStreamRequestListenResult: {_listenState}");
                }
            }
        }

        private void RPC_Connect()
        {
            EntityManager.GetName(_playerPrefab, out var prefabName);
            var ecb = new EntityCommandBuffer(WorldUpdateAllocator);

            foreach (var (rpcConnect, rpcSource, rpcEntity) in SystemAPI.Query<RefRO<RpcConnect>, RefRO<ReceiveRpcCommandRequest>>().WithEntityAccess())
            {
                Log.Debug($"Processing {rpcSource.ValueRO.SourceConnection}'s RpcConnect");

                ecb.AddComponent<NetworkStreamInGame>(rpcSource.ValueRO.SourceConnection);
                var networkId = _networkIdFromEntity[rpcSource.ValueRO.SourceConnection];

                var player = ecb.Instantiate(_playerPrefab);
                ecb.SetComponent(player, new GhostOwner { NetworkId = networkId.Value });

                //TODO: Grab Data From Database
                ecb.SetComponent(player, new PlayerName
                {
                    Value = (FixedString64Bytes)$"Player {networkId.Value}",
                });

                var localTransform = EntityManager.GetComponentData<LocalTransform>(_playerPrefab);
                ecb.SetComponent(player, new PlayerTilePosition
                {
                    Previous = localTransform.Position,
                    Target = localTransform.Position
                });

                var healthPoints = EntityManager.GetComponentData<HealthPoints>(_playerPrefab);
                ecb.SetComponent(player, new HealthPoints
                {
                    Value = healthPoints.MaxValue,
                    MaxValue = healthPoints.MaxValue
                });
                
                var damagePoints = EntityManager.GetComponentData<DamagePoints>(_playerPrefab);
                ecb.SetComponent(player, new DamagePoints
                {
                    Value = damagePoints.Value
                });

                var color = new float4(
                    UnityEngine.Random.Range(0.0f, 1.0f),
                    UnityEngine.Random.Range(0.0f, 1.0f),
                    UnityEngine.Random.Range(0.0f, 1.0f),
                    1.0f);
                var debugColor = EntityManager.GetComponentData<DebugColor>(_playerPrefab);
                ecb.SetComponent(player, new DebugColor
                {
                    Value = color,
                    StartValue = color,
                    DeathValue = debugColor.DeathValue
                });

                // Add the player to the linked entity group so it is destroyed automatically on disconnect
                ecb.AppendToBuffer(rpcSource.ValueRO.SourceConnection, new LinkedEntityGroup { Value = player });

                ecb.DestroyEntity(rpcEntity);

                Log.Debug($"Processing RpcConnect for Entity: '{rpcSource.ValueRO.SourceConnection}' " +
                    $"Added NetworkStreamInGame for NetworkId Value: '{networkId.Value}' " +
                    $"Instantiate _playerPrefab: '{prefabName}'" + $"SetComponent: new GhostOwner " +
                    $"Add LinkedEntityGroup to '{prefabName}'.");
            }
            
            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}
