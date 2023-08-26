using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst.CompilerServices;

namespace Natrium
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
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
                UnityEngine.Debug.Log($"{World.Unmanaged.Name} Drawing Gizmoz Hit at {td.end}");

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
            var mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition + new Vector3(0, 0, Camera.main.transform.position.y));

            Debug.Log($"{World.Unmanaged.Name} Input.mousePosition {Input.mousePosition}");
            Debug.Log($"{World.Unmanaged.Name} mouseWorldPosition {mouseWorldPosition}");

            ecb.AddComponent(req, new Rpc_Click { mouseWorldPosition = mouseWorldPosition });
            ecb.AddComponent<SendRpcCommandRequest>(req);

            ecb.Playback(EntityManager);
        }

        private void DrawGizmos()
        {
            //var mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition + new Vector3(0,0, Camera.main.transform.position.y));
            //var ray = Camera.main.ScreenPointToRay(Input.mousePosition + new Vector3(0, 0, Camera.main.transform.position.y));
            //Debug.Log($"Input.mousePosition {Input.mousePosition}");
            //Debug.Log($"mouseWorldPosition {mouseWorldPosition}");
            //Debug.Log($"ray {ray.origin}, {ray.GetPoint(Camera.main.transform.position.y)}");

            //mouseWorldPosition.y -= Camera.main.transform.position.y;
            //Vector3 direction = (mouseWorldPosition - Camera.main.transform.position);//.normalized * Camera.main.transform.position.y;
            //Gizmos.color = Color.red;
            //Gizmos.DrawRay(Camera.main.transform.position + new Vector3(0, -0.1f, 0), direction);
            //Debug.DrawRay(Camera.main.transform.position, direction, Color.red);

            foreach (var td in SystemAPI.Query<TouchData>().WithNone<ReceiveRpcCommandRequest>())
            {
                Gizmos.DrawCube(math.round(td.end), new float3(1, 0.1f, 1));

                //Real Ray
                //Vector3 direction = math.normalizesafe(td.end - td.start) * math.distance(td.start, td.end);
                //Debug.DrawRay(td.start, direction, Color.red);

                //Fake Ray, just used to see it in Game
                Vector3 direction = math.normalizesafe((Vector3)td.end - Camera.main.transform.position) * math.distance(Camera.main.transform.position, td.end);
                Debug.DrawRay(Camera.main.transform.position + new Vector3(0, -0.1f, 0), direction, Color.red);
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
                if (rpcClick.mouseWorldPosition.x == 0 && rpcClick.mouseWorldPosition.y == 0)
                {
                    UnityEngine.Debug.LogError($"rpcClick.mouseWorldPosition: {rpcClick.mouseWorldPosition}");
                    continue;
                }

                UnityEngine.Debug.Log($"'{World.Unmanaged.Name}' ReceiveRpcCommandRequest rpcClick.mousePosition: {rpcClick.mouseWorldPosition}");

                Entity e = EntityManager.GetBuffer<LinkedEntityGroup>(reqSrc.SourceConnection, true)[1].Value;

                var mouseWorldPosition = rpcClick.mouseWorldPosition;
                mouseWorldPosition.y = 10.0f; //ToDo: The plust 10 on y axis, comes from the offset of the camara
                float3 start = mouseWorldPosition;
                float3 end = rpcClick.mouseWorldPosition;

                ecb.AddComponent(e, new RaycastCommand { Start = start, End = end, MaxDistance = 11, reqE = reqSrc.SourceConnection });

                /*Ray ray = Camera.main.ScreenPointToRay(rpcClick.mouseWorldPosition);

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
                    UnityEngine.Debug.LogWarning($"'{World.Unmanaged.Name}' rpcClick.mousePosition: {rpcClick.mouseWorldPosition}");*/

                ecb.DestroyEntity(reqEntity);

            }

            foreach (var (ro, e) in SystemAPI.Query<RaycastOutput>().WithEntityAccess())
            {
                UnityEngine.Debug.Log($"'{World.Unmanaged.Name}' Entity {e} hit {ro.hit.Entity}");

                {
                    Entity reqE0 = EntityManager.GetBuffer<LinkedEntityGroup>(ro.reqE, true)[1].Value;
                    UnityEngine.Debug.Log($"'{World.Unmanaged.Name}' Sending TouchData to {ro.reqE}  linked with {reqE0}");
                    Entity reqE1 = EntityManager.GetBuffer<LinkedEntityGroup>(e, true)[1].Value;
                    UnityEngine.Debug.Log($"'{World.Unmanaged.Name}' {e} is linked with {reqE1}");
                }

                var req = ecb.CreateEntity();
                ecb.AddComponent(req, new TouchData { start = ro.start, end = ro.end });
                ecb.AddComponent(req, new SendRpcCommandRequest { TargetConnection = ro.reqE });

                ecb.RemoveComponent<RaycastOutput>(e);
            }

            ecb.Playback(EntityManager);
        }
    }
}