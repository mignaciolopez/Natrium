using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;
using Natrium.Gameplay.Server.Components;
using Natrium.Gameplay.Shared.Components;

namespace Natrium.Gameplay.Server.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class AimSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (clientEntity, rpcAim, rpcEntity) in SystemAPI.Query<ReceiveRpcCommandRequest, RpcAim>().WithEntityAccess())
            {
                if (rpcAim.MouseWorldPosition is { x: 0, y: 0 })
                {
                    UnityEngine.Debug.LogError($"RpcAimAttack.MouseWorldPosition: {rpcAim.MouseWorldPosition}");
                    continue;
                }

                //ghostEntity is the linked entity that contains all the prefab component, LocalTransform, Cube, Colliders, etc...
                var ghostEntity = EntityManager.GetBuffer<LinkedEntityGroup>(clientEntity.SourceConnection, true)[1].Value;

                var nid = EntityManager.GetComponentData<NetworkId>(clientEntity.SourceConnection);

                UnityEngine.Debug.Log($"'{World.Unmanaged.Name}' RpcAim from {clientEntity.SourceConnection}:{nid.Value} | MouseWorldPosition: {rpcAim.MouseWorldPosition}");

                var start = rpcAim.MouseWorldPosition;
                start.y = 10.0f; //ToDo: The plus 10 on y axis, comes from the offset of the camara
                var end = rpcAim.MouseWorldPosition;

                ecb.AddComponent(ghostEntity, new RaycastCommand { Start = start, End = end });
                ecb.DestroyEntity(rpcEntity);
            }

            foreach (var (ro, goSrc, entity) in SystemAPI.Query<RaycastOutput, GhostOwner>().WithEntityAccess())
            {
                var networkIDSource = goSrc.NetworkId;
                var networkIDTarget = 0;
                if (EntityManager.HasComponent<GhostOwner>(ro.Hit.Entity))
                    networkIDTarget = EntityManager.GetComponentData<GhostOwner>(ro.Hit.Entity).NetworkId;

                UnityEngine.Debug.Log($"'{World.Unmanaged.Name}' {entity}:{networkIDSource} hit {ro.Hit.Entity}:{networkIDTarget}");

                if (SystemAPI.HasComponent<Player>(ro.Hit.Entity))
                {
                    var rpcEntity = ecb.CreateEntity();
                    ecb.AddComponent(rpcEntity, new Attack
                    {
                        Start = ro.Start,
                        End = ro.End,
                        NetworkIdSource = networkIDSource,
                        NetworkIdTarget = networkIDTarget
                    });
                    ecb.AddComponent<SendRpcCommandRequest>(rpcEntity); //Broadcast
                }
                else
                {
                    var rpcEntity = ecb.CreateEntity();
                    ecb.AddComponent(rpcEntity, new Tile
                    {
                        Start = ro.Start,
                        End = ro.End,
                        NetworkIdSource = networkIDSource
                    });
                    foreach (var (nsig, nid, srcEntity) in SystemAPI.Query<NetworkStreamInGame, NetworkId>().WithEntityAccess())
                    {
                        if (nid.Value == networkIDSource)
                        {
                            ecb.AddComponent(rpcEntity, new SendRpcCommandRequest { TargetConnection = srcEntity });
                            break;
                        }
                    }
                }

                ecb.RemoveComponent<RaycastOutput>(entity);
            }

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}