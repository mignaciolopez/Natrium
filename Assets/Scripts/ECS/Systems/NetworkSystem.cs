using System.Security.Cryptography;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace Natrium
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial class NetworkClientSystem : SystemBase
    {
        protected override void OnCreate()
        {
            //var builder = new EntityQueryBuilder(Allocator.Temp).WithAll<NetworkId>().WithNone<NetworkStreamInGame>();
            //RequireForUpdate(GetEntityQuery(builder));

            base.OnCreate();
        }

        protected override void OnStartRunning()
        {
            EventSystem.Subscribe(Events.Client_Connect, Connect);
            EventSystem.Subscribe(Events.Client_Disconnect, Disconnect);
            base.OnStartRunning();
        }

        protected override void OnStopRunning()
        {
            EventSystem.UnSubscribe(Events.Client_Connect, Connect);
            EventSystem.UnSubscribe(Events.Client_Disconnect, Disconnect);
            base.OnStopRunning();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            
        }

        private void Connect(CustomStream stream)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            bool found = false;

            foreach (var (nId, e) in SystemAPI.Query<NetworkId>().WithEntityAccess().WithNone<NetworkStreamInGame>())
            {
                found = true;
                UnityEngine.Debug.Log($"'{World.Unmanaged.Name}' Connecting... Found Entity: {e} NetworkId: {nId.Value}");

                ecb.AddComponent<NetworkStreamInGame>(e);

                var req = ecb.CreateEntity();
                ecb.AddComponent(req, new Rpc_Connect { });
                ecb.AddComponent(req, new SendRpcCommandRequest { TargetConnection = e });
            }

            if (!found)
            {
                GameBootstrap.instance.Initialize("Default World");
                UnityEngine.Debug.LogWarning($"'{World.Unmanaged.Name}' Entity Not Found");

                var e = ecb.CreateEntity();
                ecb.AddComponent<NetworkId>(e);
                ecb.AddComponent<NetworkStreamInGame>(e);

                var req = ecb.CreateEntity();
                ecb.AddComponent(req, new Rpc_Connect { });
                ecb.AddComponent(req, new SendRpcCommandRequest { TargetConnection = e });
            }

            ecb.Playback(EntityManager);
        }

        private void Disconnect(CustomStream stream)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            bool found = false;

            foreach (var (nid, e) in SystemAPI.Query<NetworkId>().WithEntityAccess().WithAll<NetworkStreamInGame>())
            {
                UnityEngine.Debug.Log($"'{World.Unmanaged.Name}' Disconecting... Entity: {e}, NetowrkId: {nid}");
                found = true;
                //var req = ecb.CreateEntity();
                ecb.AddComponent<NetworkStreamRequestDisconnect>(e);
            }

            if(!found)
            {
                UnityEngine.Debug.LogWarning($"'{World.Unmanaged.Name}' Entity Not Found");
            }

            ecb.Playback(EntityManager);
        }
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class NetworkServertSystem : SystemBase
    {
        private ComponentLookup<NetworkId> mNetworkIdFromEntity;

        protected override void OnCreate()
        {
            RequireForUpdate<PlayerSpawnerData>();

            var builder = new EntityQueryBuilder(Allocator.Temp).WithAll<Rpc_Connect>().WithAll<ReceiveRpcCommandRequest>();
            RequireForUpdate(GetEntityQuery(builder));

            mNetworkIdFromEntity = GetComponentLookup<NetworkId>(true);

            base.OnCreate();
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
        }

        protected override void OnStopRunning()
        {
            base.OnStopRunning();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            mNetworkIdFromEntity.Update(this);
            RPC_Connect();
        }

        private void RPC_Connect()
        {
            var prefab = SystemAPI.GetSingleton<PlayerSpawnerData>().playerPrefab;

            EntityManager.GetName(prefab, out var prefabName);

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (reqSrc, reqEntity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>>().WithAll<Rpc_Connect>().WithEntityAccess())
            {
                ecb.AddComponent<NetworkStreamInGame>(reqSrc.ValueRO.SourceConnection);
                var networkId = mNetworkIdFromEntity[reqSrc.ValueRO.SourceConnection];

                var player = ecb.Instantiate(prefab);
                ecb.SetComponent(player, new GhostOwner { NetworkId = networkId.Value });

                // Add the player to the linked entity group so it is destroyed automatically on disconnect
                ecb.AppendToBuffer(reqSrc.ValueRO.SourceConnection, new LinkedEntityGroup { Value = player });

                ecb.DestroyEntity(reqEntity);

                UnityEngine.Debug.Log($"'{World.Unmanaged.Name}' Processing ReceiveRpcCommandRequest for Entity: '{reqSrc.ValueRO.SourceConnection}' " +
                    $"Added NetworkStreamInGame for NetworkId Value: '{networkId.Value}' " +
                    $"Instantiate prefab: '{prefabName}'" + $"SetComponent: new GhostOwner " +
                    $"Add LinkedEntityGroup to '{prefabName}'.");
            }
            ecb.Playback(EntityManager);
        }
    }
}