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
    [UpdateInGroup(typeof(GameplaySystemGroup))]
    [CreateAfter(typeof(SharedSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class ServerSystem : SystemBase
    {
        private static ComponentLookup<NetworkId> _networkIdFromEntity;
        private NetworkStreamRequestListenResult.State _listenState;

        private EntityCommandBuffer _ecb;

        protected override void OnCreate()
        {
            base.OnCreate();

            RequireForUpdate<SystemsSettings>();

            _networkIdFromEntity = GetComponentLookup<NetworkId>(true);
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();

            if (SystemAPI.TryGetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>(out var ecbs))
                _ecb = ecbs.CreateCommandBuffer(World.Unmanaged);

            Listen();
        }

        protected override void OnStopRunning()
        {
            base.OnStopRunning();
        }

        protected override void OnUpdate()
        {
            if (SystemAPI.TryGetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>(out var ecbs))
                _ecb = ecbs.CreateCommandBuffer(World.Unmanaged);

            _networkIdFromEntity.Update(this);

            GetListeningStatus();
            RPC_Connect();

            //TODO: delete changemovement()
            //changemovement();
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

                    var req = _ecb.CreateEntity();
                    _ecb.AddComponent(req, new NetworkStreamRequestListen
                    {
                        Endpoint = endpoint
                    });
                    _ecb.AddComponent<NetworkStreamRequestListenResult>(req);
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
            var prefab = SystemAPI.GetSingleton<PlayerSpawnerData>().PlayerPrefab;

            EntityManager.GetName(prefab, out var prefabName);

            foreach (var (rpcConnect, rpcSource, rpcEntity) in SystemAPI.Query<RefRO<RpcConnect>, RefRO<ReceiveRpcCommandRequest>>().WithEntityAccess())
            {
                Log.Debug($"Processing {rpcSource.ValueRO.SourceConnection}'s RpcConnect");

                _ecb.AddComponent<NetworkStreamInGame>(rpcSource.ValueRO.SourceConnection);
                var networkId = _networkIdFromEntity[rpcSource.ValueRO.SourceConnection];

                var player = _ecb.Instantiate(prefab);
                _ecb.SetComponent(player, new GhostOwner { NetworkId = networkId.Value });

                //TODO: Grab Data From Database
                var position = new float3(5.0f, 1.0f, 5.0f);
                _ecb.SetComponent(player, new LocalTransform
                {
                    Position = position,
                    Rotation = quaternion.identity,
                    Scale = 1.0f
                });
                _ecb.SetComponent(player, new PlayerName
                {
                    Value = (FixedString64Bytes)$"Player {networkId.Value}",
                });

                _ecb.SetComponent(player, new PlayerTilePosition
                {
                    Previous = position,
                    Target = position
                });

                _ecb.SetComponent(player, new Health() { Value = 100 });
                _ecb.SetComponent(player, new MaxHealth() { Value = 100 });
                _ecb.SetComponent(player, new DamagePoints() { Value = 10.0f });

                var color = new float3(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f));

                _ecb.SetComponent(player, new DebugColor
                {
                    Value = color
                });

                // Add the player to the linked entity group so it is destroyed automatically on disconnect
                _ecb.AppendToBuffer(rpcSource.ValueRO.SourceConnection, new LinkedEntityGroup { Value = player });

                _ecb.DestroyEntity(rpcEntity);

                Log.Debug($"Processing RpcConnect for Entity: '{rpcSource.ValueRO.SourceConnection}' " +
                    $"Added NetworkStreamInGame for NetworkId Value: '{networkId.Value}' " +
                    $"Instantiate prefab: '{prefabName}'" + $"SetComponent: new GhostOwner " +
                    $"Add LinkedEntityGroup to '{prefabName}'.");
            }
        }
    }
}
