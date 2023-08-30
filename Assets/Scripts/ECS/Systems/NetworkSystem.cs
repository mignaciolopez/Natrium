using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Scenes;

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
                ecb.AddComponent<GhostOwnerIsLocal>(e);

                var req = ecb.CreateEntity();
                ecb.AddComponent(req, new RpcConnect { });
                ecb.AddComponent(req, new SendRpcCommandRequest { TargetConnection = e });
            }

            if (!found)
            {
                UnityEngine.Debug.LogWarning($"'{World.Unmanaged.Name}' Entity Not Found");
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
                //ecb.AddComponent<NetworkStreamRequestDisconnect>(e);
            }

            if(!found)
            {
                UnityEngine.Debug.LogWarning($"'{World.Unmanaged.Name}' Entity Not Found");
            }

            ecb.Playback(EntityManager);
        }
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class NetworkServerSystem : SystemBase
    {
        public static ComponentLookup<NetworkId> mNetworkIdFromEntity;

        private struct LinkedEntities
        {
            public int networkID;
            public Entity entity;
            public Entity playerPrefabEntity;
        }

        private Dictionary<int, LinkedEntities> m_ConnectedClients;

        protected override void OnCreate()
        {
            //RequireForUpdate<PlayerSpawnerData>();

            //var builder = new EntityQueryBuilder(Allocator.Temp).WithAll<Rpc_Connect>().WithAll<ReceiveRpcCommandRequest>();
            //RequireForUpdate(GetEntityQuery(builder));

            mNetworkIdFromEntity = GetComponentLookup<NetworkId>(true);

            base.OnCreate();
        }

        protected override void OnStartRunning()
        {
            m_ConnectedClients = new Dictionary<int, LinkedEntities>();
            base.OnStartRunning();
        }

        protected override void OnStopRunning()
        {
            m_ConnectedClients = null;
            base.OnStopRunning();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            mNetworkIdFromEntity.Update(this);
            RPC_Connect();

            foreach (var (ro, e) in SystemAPI.Query<RaycastOutput>().WithEntityAccess())
            {
                UnityEngine.Debug.Log($"'{World.Unmanaged.Name}' Entity {e} hit {ro.Hit.Entity}");

                {
                    Entity reqE0 = EntityManager.GetBuffer<LinkedEntityGroup>(ro.ReqE, true)[1].Value;
                    UnityEngine.Debug.Log($"'{World.Unmanaged.Name}' Sending TouchData to {ro.ReqE}  linked with {reqE0}");
                    Entity reqE1 = EntityManager.GetBuffer<LinkedEntityGroup>(e, true)[1].Value;
                    UnityEngine.Debug.Log($"'{World.Unmanaged.Name}' {e} is linked with {reqE1}");
                }


                int nidSource = 0;
                if (EntityManager.HasComponent<NetworkId>(ro.ReqE))
                    nidSource = mNetworkIdFromEntity[ro.ReqE].Value;

                int nidTarget = 0;
                if (EntityManager.HasComponent<NetworkId>(ro.Hit.Entity))
                    nidTarget = mNetworkIdFromEntity[ro.Hit.Entity].Value;

                var req = ecb.CreateEntity();
                ecb.AddComponent(req, new TouchData { Start = ro.Start, End = ro.End, NetworkIDSource = nidSource, NetworkIDTarget = nidTarget });
                ecb.AddComponent(req, new SendRpcCommandRequest { });

                ecb.RemoveComponent<RaycastOutput>(e);
            }

            ecb.Playback(EntityManager);
        }

        private void RPC_Connect()
        {
            var prefab = SystemAPI.GetSingleton<PlayerSpawnerData>().playerPrefab;

            EntityManager.GetName(prefab, out var prefabName);

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (reqSrc, reqEntity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>>().WithAll<RpcConnect>().WithEntityAccess())
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