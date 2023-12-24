using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Natrium.Gameplay.Server.Components;
using Natrium.Gameplay.Shared.Components;
using Unity.Mathematics;
using Unity.Transforms;
using Natrium.Shared;
using Natrium.Gameplay.Shared;
using Natrium.Gameplay.Client.Components;

namespace Natrium.Gameplay.Server.Systems
{
    [UpdateInGroup(typeof(GameplaySystemGroup))]
    [CreateAfter(typeof(SharedSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class ServerSystem : SystemBase
    {
        private static ComponentLookup<NetworkId> _networkIdFromEntity;

        protected override void OnCreate()
        {
            base.OnCreate();
            //RequireForUpdate<PlayerSpawnerData>();

            //var builder = new EntityQueryBuilder(Allocator.Temp).WithAll<Rpc_Connect>().WithAll<ReceiveRpcCommandRequest>();
            //RequireForUpdate(GetEntityQuery(builder));

            _networkIdFromEntity = GetComponentLookup<NetworkId>(true);

        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();

            var client = SystemAPI.GetSingleton<ClientConnectionData>();

            using var query = WorldManager.ServerWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            query.GetSingletonRW<NetworkStreamDriver>().ValueRW.Listen((ClientServerBootstrap.DefaultListenAddress.WithPort(client.Port)));
        }

        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            _networkIdFromEntity.Update(this);

            RPC_Connect();

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }

        private void RPC_Connect()
        {
            var prefab = SystemAPI.GetSingleton<PlayerSpawnerData>().PlayerPrefab;

            EntityManager.GetName(prefab, out var prefabName);

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (reqSrc, reqEntity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>>().WithAll<RpcConnect>().WithEntityAccess())
            {
                UnityEngine.Debug.Log($"{World.Unmanaged.Name} processing {reqSrc.ValueRO.SourceConnection}'s RpcConnect");

                ecb.AddComponent<NetworkStreamInGame>(reqSrc.ValueRO.SourceConnection);
                var networkId = _networkIdFromEntity[reqSrc.ValueRO.SourceConnection];

                var player = ecb.Instantiate(prefab);
                ecb.SetComponent(player, new GhostOwner { NetworkId = networkId.Value });

                //TODO: Grab Data From Database
                var position = new float3(5.0f, 0.0f, 5.0f);
                ecb.SetComponent(player, new LocalTransform
                {
                    Position = position,
                    Rotation = quaternion.identity,
                    Scale = 1.0f
                });
                ecb.SetComponent(player, new Player
                {
                    Name = (FixedString64Bytes)$"Player {networkId.Value}",
                    PreviousPos = (int3)position,
                    NextPos = (int3)position
                });

                ecb.SetComponent(player, new Health() { Value = 100 });
                ecb.SetComponent(player, new MaxHealth() { Value = 100 });
                ecb.SetComponent(player, new DamagePoints() { Value = 10.0f });

                // Add the player to the linked entity group so it is destroyed automatically on disconnect
                ecb.AppendToBuffer(reqSrc.ValueRO.SourceConnection, new LinkedEntityGroup { Value = player });

                ecb.DestroyEntity(reqEntity);

                UnityEngine.Debug.Log($"'{World.Unmanaged.Name}' Processing ReceiveRpcCommandRequest for Entity: '{reqSrc.ValueRO.SourceConnection}' " +
                    $"Added NetworkStreamInGame for NetworkId Value: '{networkId.Value}' " +
                    $"Instantiate prefab: '{prefabName}'" + $"SetComponent: new GhostOwner " +
                    $"Add LinkedEntityGroup to '{prefabName}'.");
            }
            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}
