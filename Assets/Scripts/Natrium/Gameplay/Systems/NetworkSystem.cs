using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Natrium.Gameplay.Components;
using Natrium.Shared.Systems;
using Unity.Mathematics;
using Unity.Transforms;

namespace Natrium.Gameplay.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial class NetworkClientSystem : SystemBase
    {
        protected override void OnStartRunning()
        {
            EventSystem.Subscribe(Shared.Events.OnClientConnect, OnClientConnect);
            EventSystem.Subscribe(Shared.Events.OnClientDisconnect, OnClientDisconnect);
            base.OnStartRunning();
        }

        protected override void OnStopRunning()
        {
            EventSystem.UnSubscribe(Shared.Events.OnClientConnect, OnClientConnect);
            EventSystem.UnSubscribe(Shared.Events.OnClientDisconnect, OnClientDisconnect);
            base.OnStopRunning();
        }

        protected override void OnUpdate()
        {
            
        }

        private void OnClientConnect(Shared.Stream stream)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            var found = false;

            foreach (var (nId, e) in SystemAPI.Query<NetworkId>().WithEntityAccess().WithNone<NetworkStreamInGame>())
            {
                found = true;
                UnityEngine.Debug.Log($"'{World.Unmanaged.Name}' Connecting... Found Entity: {e} NetworkId: {nId.Value}");

                ecb.AddComponent<NetworkStreamInGame>(e);
                ecb.AddComponent<GhostOwnerIsLocal>(e);

                var req = ecb.CreateEntity();
                ecb.AddComponent<RpcConnect>(req);
                ecb.AddComponent(req, new SendRpcCommandRequest { TargetConnection = e });
            }

            if (!found)
            {
                UnityEngine.Debug.LogWarning($"'{World.Unmanaged.Name}' Entity Not Found");
            }

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }

        private void OnClientDisconnect(Shared.Stream stream)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            var found = false;

            foreach (var (nid, e) in SystemAPI.Query<NetworkId>().WithEntityAccess().WithAll<NetworkStreamInGame>())
            {
                UnityEngine.Debug.Log($"'{World.Unmanaged.Name}' Disconnecting... Entity: {e}, NetworkId: {nid}");
                found = true;
                //var req = ecb.CreateEntity();
                //ecb.AddComponent<NetworkStreamRequestDisconnect>(e);
            }

            if(!found)
            {
                UnityEngine.Debug.LogWarning($"'{World.Unmanaged.Name}' Entity Not Found");
            }

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class NetworkServerSystem : SystemBase
    {
        private static ComponentLookup<NetworkId> _networkIdFromEntity;
        
        protected override void OnCreate()
        {
            //RequireForUpdate<PlayerSpawnerData>();

            //var builder = new EntityQueryBuilder(Allocator.Temp).WithAll<Rpc_Connect>().WithAll<ReceiveRpcCommandRequest>();
            //RequireForUpdate(GetEntityQuery(builder));

            _networkIdFromEntity = GetComponentLookup<NetworkId>(true);

            base.OnCreate();
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
                    Name = (FixedString64Bytes)($"Player {networkId}"), 
                    PreviousPos = (int3)position,
                    NextPos = (int3)position
                });
                
                ecb.SetComponent(player, new Health() { Value = 100 } );
                ecb.SetComponent(player, new MaxHealth() { Value = 100 } );
                ecb.SetComponent(player, new DamagePoints() { Value = 10.0f } );

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