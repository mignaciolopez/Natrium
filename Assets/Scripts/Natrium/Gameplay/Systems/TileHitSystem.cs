using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Natrium.Gameplay.Components;
using Natrium.Shared.Systems;
using Unity.Transforms;

namespace Natrium.Gameplay.Systems
{
    #region Client
    
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class TileHitSystemClient : SystemBase
    {
        protected override void OnStartRunning()
        {
            EventSystem.Subscribe(Shared.Events.OnPrimaryClick, OnPrimaryClick);
            Shared.Gizmos.OnEcsGizmos.AddListener(DrawGizmos);

            base.OnStartRunning();
        }

        protected override void OnStopRunning()
        {
            EventSystem.UnSubscribe(Shared.Events.OnPrimaryClick, OnPrimaryClick);
            Shared.Gizmos.OnEcsGizmos.RemoveListener(DrawGizmos);

            base.OnStopRunning();
        }

        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (rpcHit, rpcEntity) in SystemAPI.Query<RpcHit>().WithAll<ReceiveRpcCommandRequest>().WithEntityAccess())
            {
                foreach (var (go, ghostEntity) in SystemAPI.Query<GhostOwner>().WithEntityAccess())
                {
                    if (go.NetworkId != rpcHit.NetworkIdSource) continue;
                    
                    Debug.Log($"{World.Unmanaged.Name} New Hit from {ghostEntity}:{go.NetworkId} at ({rpcHit.End}) colliding with {rpcHit.NetworkIdTarget}");
                    ecb.AddComponent(ghostEntity, new Hit { End = rpcHit.End, NetworkIdTarget = rpcHit.NetworkIdTarget });
                    break;
                }
                
                ecb.DestroyEntity(rpcEntity);
            }

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }

        private void OnPrimaryClick(Shared.Stream stream)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

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
            ecb.Dispose();
        }

        private void DrawGizmos()
        {
            foreach (var (hit, lt) in SystemAPI.Query<Hit, LocalTransform>().WithAll<GhostOwner>())
            {
                Gizmos.color = Color.gray;
                Gizmos.DrawCube(math.round(hit.End), new float3(1, 0.1f, 1));

                var end = float3.zero;
                
                if (hit.NetworkIdTarget != 0)
                {
                    foreach (var (goTarget, ltTarget) in SystemAPI.Query<GhostOwner, LocalTransform>())
                    {
                        if (goTarget.NetworkId != hit.NetworkIdTarget) continue;

                        end = ltTarget.Position;
                        break;
                    }
                }
                else
                {
                    end = hit.End;
                }

                if (end is not { x: 0, y: 0, z: 0 })
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawLine(lt.Position, end);
                }
            }
        }
    }

    #endregion


    #region Server

    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class TileHitSystemServer : SystemBase
    {
        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (clientEntity, rpcClick, rpcEntity) in SystemAPI.Query<ReceiveRpcCommandRequest, RpcClick>().WithEntityAccess())
            {
                if (rpcClick.MouseWorldPosition is { x: 0, y: 0 })
                {
                    Debug.LogError($"RpcClick.MouseWorldPosition: {rpcClick.MouseWorldPosition}");
                    continue;
                }

                Debug.Log($"'{World.Unmanaged.Name}' ReceiveRpcCommandRequest RpcClick | MouseWorldPosition: {rpcClick.MouseWorldPosition}");

                //ghostEntity is the linked entity that contains all the prefab component, LocalTransform, Cube, Colliders, etc...
                var ghostEntity = EntityManager.GetBuffer<LinkedEntityGroup>(clientEntity.SourceConnection, true)[1].Value;
                
                var start = rpcClick.MouseWorldPosition;
                start.y = 10.0f; //ToDo: The plus 10 on y axis, comes from the offset of the camara
                var end = rpcClick.MouseWorldPosition;

                ecb.AddComponent(ghostEntity, new Components.RaycastCommand { Start = start, End = end});

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

                ecb.DestroyEntity(rpcEntity);

            }
            
            foreach (var (ro, goSrc, entity) in SystemAPI.Query<RaycastOutput, GhostOwner>().WithEntityAccess())
            {
                var networkIDSource = goSrc.NetworkId;
                var networkIDTarget = 0;
                if (EntityManager.HasComponent<GhostOwner>(ro.Hit.Entity))
                    networkIDTarget = EntityManager.GetComponentData<GhostOwner>(ro.Hit.Entity).NetworkId;
                
                Debug.Log($"'{World.Unmanaged.Name}' {entity}:{networkIDSource} hit {ro.Hit.Entity}:{networkIDTarget}");

                var rpcEntity = ecb.CreateEntity();
                ecb.AddComponent(rpcEntity, new RpcHit
                {
                    Start = ro.Start, 
                    End = ro.End, 
                    NetworkIdSource = networkIDSource, 
                    NetworkIdTarget = networkIDTarget
                });
                ecb.AddComponent<SendRpcCommandRequest>(rpcEntity); //Broadcast

                ecb.RemoveComponent<RaycastOutput>(entity);
            }

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
    
    #endregion
}