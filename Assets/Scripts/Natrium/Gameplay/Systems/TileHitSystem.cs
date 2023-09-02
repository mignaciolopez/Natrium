using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Natrium.Gameplay.Components;
using Natrium.Shared;
using Natrium.Shared.Systems;

namespace Natrium.Gameplay.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class TileHitSystemClient : SystemBase
    {
        protected override void OnStartRunning()
        {
            EventSystem.Subscribe(Events.OnPrimaryClick, OnPrimaryClick);
            Shared.Gizmos.OnEcsGizmos.AddListener(DrawGizmos);

            base.OnStartRunning();
        }

        protected override void OnStopRunning()
        {
            EventSystem.UnSubscribe(Events.OnPrimaryClick, OnPrimaryClick);
            Shared.Gizmos.OnEcsGizmos.RemoveListener(DrawGizmos);

            base.OnStopRunning();
        }

        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (td, reqEntity) in SystemAPI.Query<TouchData>().WithAll<ReceiveRpcCommandRequest>().WithEntityAccess())
            {
                Debug.Log($"{World.Unmanaged.Name} Drawing Gizmos Hit at {td.End}");

                ecb.RemoveComponent<ReceiveRpcCommandRequest>(reqEntity);
            }

            ecb.Playback(EntityManager);
        }

        private void OnPrimaryClick(Stream stream)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (td, e) in SystemAPI.Query<TouchData>().WithNone<ReceiveRpcCommandRequest>().WithEntityAccess())
            {
                Debug.Log($"Buffering Destroy of Entity:{e}, to remove TouchData{td} from screen.");
                ecb.DestroyEntity(e);
            }

            if (Camera.main != null)
            {
                var mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition + new Vector3(0, 0, Camera.main.transform.position.y));

                Debug.Log($"{World.Unmanaged.Name} Input.mousePosition {Input.mousePosition}");
                Debug.Log($"{World.Unmanaged.Name} mouseWorldPosition {mouseWorldPosition}");

                var req = ecb.CreateEntity();
                ecb.AddComponent(req, new RpcClick { MouseWorldPosition = mouseWorldPosition });
                ecb.AddComponent<SendRpcCommandRequest>(req);
            }

            ecb.Playback(EntityManager);
        }

        private void DrawGizmos()
        {
            //var mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition + new Vector3(0,0, Camera.main.transform.position.y));
            //var ray = Camera.main.ScreenPointToRay(Input.mousePosition + new Vector3(0, 0, Camera.main.transform.position.y));
            //Debug.Log($"OnDrawGizmos");
            //Debug.Log($"mouseWorldPosition {mouseWorldPosition}");
            //Debug.Log($"ray {ray.origin}, {ray.GetPoint(Camera.main.transform.position.y)}");

            //mouseWorldPosition.y -= Camera.main.transform.position.y;
            //Vector3 direction = (mouseWorldPosition - Camera.main.transform.position);//.normalized * Camera.main.transform.position.y;
            //Gizmos.color = Color.red;
            //Gizmos.DrawRay(Camera.main.transform.position + new Vector3(0, -0.1f, 0), direction);
            //Debug.DrawRay(Camera.main.transform.position, direction, Color.red);

            foreach (var td in SystemAPI.Query<TouchData>().WithNone<ReceiveRpcCommandRequest>())
            {
                UnityEngine.Gizmos.DrawCube(math.round(td.End), new float3(1, 0.1f, 1));

                /*var entSource = Entity.Null;
                var entTarget = Entity.Null;

                foreach (var (nid, lt, e) in SystemAPI.Query<NetworkId, LocalTransform>().WithEntityAccess())
                {
                    switch (td.NetworkIDSource)
                    {
                        case nid.Value:
                            entSource = e;
                            continue;
                        default:
                            break;
                    }
                    if (td.NetworkIDSource != 0)
                    {
                        if (nid.Value == td.NetworkIDSource)
                        {
                            
                        }
                    }
                    if (td.NetworkIDTarget != 0)
                    {
                        if (nid.Value == td.NetworkIDTarget)
                        {
                            entTarget = e;
                            continue;
                        }
                    }
                }

                //Fake Ray, just used to see it in Game
                float3 ltSrc = float3.zero;
                float3 ltTarget = float3.zero;

                if (EntityManager.Exists(entSource))
                {
                    ltSrc = EntityManager.GetComponentData<LocalTransform>(entSource).Position;
                }
                else
                {
                    //Warning
                }

                if (EntityManager.Exists(entTarget))
                {
                    ltTarget = EntityManager.GetComponentData<LocalTransform>(entTarget).Position;
                }
                else
                {
                    ltTarget = td.End;
                }
                

                Vector3 direction = math.normalizesafe(ltTarget - ltSrc) * math.distance(ltSrc, ltTarget);
                Debug.DrawRay(ltSrc, direction, Color.red);*/

                //Real Ray
                //Vector3 direction = math.normalizesafe(td.end - td.start) * math.distance(td.start, td.end);
                //Debug.DrawRay(td.start, direction, Color.red);

                //Fake Ray, just used to see it in Game
                //Vector3 direction = math.normalizesafe((Vector3)td.end - Camera.main.transform.position) * math.distance(Camera.main.transform.position, td.end);
                //Debug.DrawRay(Camera.main.transform.position + new Vector3(0, -0.1f, 0), direction, Color.red);
            }
        }
    }




    //Server

    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class TileHitSystemServer : SystemBase
    {
        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (reqSrc, rpcClick, reqEntity) in SystemAPI.Query<ReceiveRpcCommandRequest, RpcClick>().WithEntityAccess())
            {
                if (rpcClick.MouseWorldPosition is { x: 0, y: 0 })
                {
                    Debug.LogError($"RpcClick.MouseWorldPosition: {rpcClick.MouseWorldPosition}");
                    continue;
                }

                Debug.Log($"'{World.Unmanaged.Name}' ReceiveRpcCommandRequest RpcClick.MouseWorldPosition: {rpcClick.MouseWorldPosition}");

                var e = EntityManager.GetBuffer<LinkedEntityGroup>(reqSrc.SourceConnection, true)[1].Value;

                var mouseWorldPosition = rpcClick.MouseWorldPosition;
                mouseWorldPosition.y = 10.0f; //ToDo: The plus 10 on y axis, comes from the offset of the camara
                var start = mouseWorldPosition;
                var end = rpcClick.MouseWorldPosition;

                ecb.AddComponent(e, new Components.RaycastCommand { Start = start, End = end, MaxDistance = 11, ReqE = reqSrc.SourceConnection });

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
                        UnityEngine.Debug.Log($"'{World.Unmanaged.Name}' Hit Float at: {hit.point}");
                        var req = ecb.CreateEntity();
                        ecb.AddComponent(req, new TouchData { tile = (int3)math.round(hit.point) });
                        ecb.AddComponent(req, new SendRpcCommandRequest { TargetConnection = reqSrc.SourceConnection });
                    }
                }
                else
                    UnityEngine.Debug.LogWarning($"'{World.Unmanaged.Name}' rpcClick.mousePosition: {rpcClick.mouseWorldPosition}");*/

                ecb.DestroyEntity(reqEntity);

            }

            ecb.Playback(EntityManager);
        }
    }
}