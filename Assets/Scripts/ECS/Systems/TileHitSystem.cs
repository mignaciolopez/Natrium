using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;

namespace Natrium
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial class TileHitSystemClient : SystemBase
    {
        protected override void OnCreate()
        {
            base.OnCreate();
        }

        protected override void OnStartRunning()
        {
            EventSystem.Subscribe(Events.OnPrimaryClick, On_Click);
            base.OnStartRunning();
        }

        protected override void OnStopRunning()
        {
            EventSystem.UnSubscribe(Events.OnPrimaryClick, On_Click);
            base.OnStopRunning();
        }

        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var prefab = SystemAPI.GetSingleton<PlayerSpawnerData>().hitTilePrefab;

            foreach (var (td, e) in SystemAPI.Query<TouchData>().WithEntityAccess())
            {
                ecb.DestroyEntity(e);
                
                var hitEntity = ecb.Instantiate(prefab);
                ecb.SetComponent(hitEntity, LocalTransform.FromPosition(td.tile));

                UnityEngine.Debug.Log($"TouchData Entity: {e}, Instatiating Hit at {td.tile}, Hit Entity {hitEntity}");
            }

            ecb.Playback(EntityManager);
        }

        private void On_Click(CustomStream stream = null)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (nid, e) in SystemAPI.Query<NetworkId>().WithAll<GhostOwnerIsLocal, NetworkStreamInGame>().WithEntityAccess())
            {
                Debug.Log($"'{World.Unmanaged.Name}' {nid.Value} Sending Rpc_Click");

                var req = ecb.CreateEntity();
                ecb.AddComponent<Rpc_Click>(req);
                ecb.AddComponent(req, new SendRpcCommandRequest { TargetConnection = e });
            }

            ecb.Playback(EntityManager);
        }
    }






    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class TileHitSystemServer : SystemBase
    {
        protected override void OnCreate()
        {
            base.OnCreate();
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
        }

        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (reqSrc, reqEntity) in SystemAPI.Query<ReceiveRpcCommandRequest>().WithAll<Rpc_Click>().WithEntityAccess())
            {
                Entity e = EntityManager.GetBuffer<LinkedEntityGroup>(reqSrc.SourceConnection, true)[1].Value;
                var lt = EntityManager.GetComponentData<LocalTransform>(e);
                var pid = EntityManager.GetComponentData<PlayerInputData>(e);

                float3 origin = lt.Position + new float3(0, 6, 0);
                Camera.main.transform.position = origin;

                Ray ray = Camera.main.ScreenPointToRay(pid.LastScreenCoordinates);
                if (Physics.Raycast(ray, out RaycastHit hit, 10.0f))
                {
                    UnityEngine.Debug.Log($"'{World.Unmanaged.Name}' HIT! Received Click: {pid.LastScreenCoordinates}");

                    var req = ecb.CreateEntity();
                    ecb.AddComponent(req, new TouchData { tile = (int3)math.round(hit.point) });
                    ecb.AddComponent(req, new SendRpcCommandRequest { TargetConnection = reqSrc.SourceConnection });
                }
                else
                    UnityEngine.Debug.LogWarning($"'{World.Unmanaged.Name}' Received Click: {pid.LastScreenCoordinates}");

                ecb.DestroyEntity(reqEntity);

            }

            ecb.Playback(EntityManager);
        }
    }
}