using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using System.Linq;

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
            EventSystem.Subscribe(Events.OnPrimaryClick, OnPrimaryClick);
            Gizmo.s_OnDrawGizmos.AddListener(DrawGizmos);

            base.OnStartRunning();
        }

        protected override void OnStopRunning()
        {
            EventSystem.UnSubscribe(Events.OnPrimaryClick, OnPrimaryClick);
            Gizmo.s_OnDrawGizmos.RemoveListener(DrawGizmos);

            base.OnStopRunning();
        }

        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (reqSrc, td, reqEntity) in SystemAPI.Query<ReceiveRpcCommandRequest, TouchData>().WithEntityAccess())
            {
                UnityEngine.Debug.Log($"{World.Unmanaged.Name} Drawing Gizmoz Hit at {td.tile}");
                ecb.RemoveComponent<ReceiveRpcCommandRequest>(reqEntity);
            }

            ecb.Playback(EntityManager);
        }

        public void OnPrimaryClick(CustomStream strem)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (td2, e) in SystemAPI.Query<TouchData>().WithNone<ReceiveRpcCommandRequest>().WithEntityAccess())
            {
                ecb.DestroyEntity(e);
            }

            var req = ecb.CreateEntity();
            ecb.AddComponent(req, new Rpc_Click { mousePosition = Input.mousePosition });
            ecb.AddComponent<SendRpcCommandRequest>(req);

            ecb.Playback(EntityManager);
        }

        private void DrawGizmos()
        {
            foreach (var td in SystemAPI.Query<TouchData>().WithNone<ReceiveRpcCommandRequest>())
            {
                Gizmos.DrawCube((float3)td.tile, new float3(1, 0.1f, 1));
            }
        }
    }




    //Server

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

            foreach (var (reqSrc, rpcClick, reqEntity) in SystemAPI.Query<ReceiveRpcCommandRequest, Rpc_Click>().WithEntityAccess())
            {
                UnityEngine.Debug.Log($"'{World.Unmanaged.Name}' ReceiveRpcCommandRequest rpcClick.mousePosition: {rpcClick.mousePosition}");

                Entity e = EntityManager.GetBuffer<LinkedEntityGroup>(reqSrc.SourceConnection, true)[1].Value;
                var lt = EntityManager.GetComponentData<LocalTransform>(e);

                float3 origin = lt.Position + new float3(0, 6, 0);
                Camera.main.transform.position = origin;

                if(rpcClick.mousePosition.x == 0 && rpcClick.mousePosition.y == 0)
                {
                    UnityEngine.Debug.LogError($"rpcClick.mousePosition: {rpcClick.mousePosition}");
                    continue;
                }

                Ray ray = Camera.main.ScreenPointToRay(rpcClick.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 10.0f))
                {
                    if (hit.collider.gameObject.CompareTag("Player"))
                    {
                        UnityEngine.Debug.Log($"'{World.Unmanaged.Name}' Hit Entity");
                        //var req = ecb.CreateEntity();
                        //ecb.AddComponent(req, new HitEntity { entity =  });
                        //ecb.AddComponent(req, new SendRpcCommandRequest { TargetConnection = reqSrc.SourceConnection });
                    }
                    else 
                    {
                        UnityEngine.Debug.Log($"'{World.Unmanaged.Name}' Hit Floot at: {hit.point}");
                        var req = ecb.CreateEntity();
                        ecb.AddComponent(req, new TouchData { tile = (int3)math.round(hit.point) });
                        ecb.AddComponent(req, new SendRpcCommandRequest { TargetConnection = reqSrc.SourceConnection });
                    }
                }
                else
                    UnityEngine.Debug.LogWarning($"'{World.Unmanaged.Name}' rpcClick.mousePosition: {rpcClick.mousePosition}");

                ecb.DestroyEntity(reqEntity);

            }

            ecb.Playback(EntityManager);
        }
    }
}