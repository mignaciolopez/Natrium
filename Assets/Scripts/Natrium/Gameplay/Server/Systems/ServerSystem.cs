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

        EntityCommandBuffer _ecb;
        protected override void OnCreate()
        {
            base.OnCreate();
            
            RequireForUpdate<ServerSystemExecute>();
            RequireForUpdate<SystemsSettings>();

            //var builder = new EntityQueryBuilder(Allocator.Temp).WithAll<Rpc_Connect>().WithAll<ReceiveRpcCommandRequest>();
            //RequireForUpdate(GetEntityQuery(builder));

            _networkIdFromEntity = GetComponentLookup<NetworkId>(true);
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();

            _ecb = new EntityCommandBuffer(Allocator.Temp);

            Listen();

            _ecb.Playback(EntityManager);
            _ecb.Dispose();
        }

        protected override void OnStopRunning()
        {
            base.OnStopRunning();
        }

        protected override void OnUpdate()
        {
            _ecb = new EntityCommandBuffer(Allocator.Temp);

            _networkIdFromEntity.Update(this);

            GetListeningStatus();
            RPC_Connect();

            //delete changemovement
            //changemovement();

            _ecb.Playback(EntityManager);
            _ecb.Dispose();
        }

        //delete changemovement
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
        //delete changemovement

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

            foreach (var (reqSrc, reqEntity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>>().WithAll<RpcConnect>().WithEntityAccess())
            {
                Log.Debug($"Processing {reqSrc.ValueRO.SourceConnection}'s RpcConnect");

                _ecb.AddComponent<NetworkStreamInGame>(reqSrc.ValueRO.SourceConnection);
                var networkId = _networkIdFromEntity[reqSrc.ValueRO.SourceConnection];

                var player = EntityManager.Instantiate(prefab);
                _ecb.SetComponent(player, new GhostOwner { NetworkId = networkId.Value });

                //TODO: Grab Data From Database
                var position = new float3(5.0f, 0.0f, 5.0f);
                _ecb.SetComponent(player, new LocalTransform
                {
                    Position = position,
                    Rotation = quaternion.identity,
                    Scale = 1.0f
                });
                _ecb.SetComponent(player, new Player
                {
                    Name = (FixedString64Bytes)$"Player {networkId.Value}",
                    PreviousPos = (int3)position,
                    NextPos = (int3)position
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
                _ecb.AppendToBuffer(reqSrc.ValueRO.SourceConnection, new LinkedEntityGroup { Value = player });

                _ecb.DestroyEntity(reqEntity);

                Log.Debug($"Processing RpcConnect for Entity: '{reqSrc.ValueRO.SourceConnection}' " +
                    $"Added NetworkStreamInGame for NetworkId Value: '{networkId.Value}' " +
                    $"Instantiate prefab: '{prefabName}'" + $"SetComponent: new GhostOwner " +
                    $"Add LinkedEntityGroup to '{prefabName}'.");
            }
        }
    }
}
