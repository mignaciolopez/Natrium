using Unity.Entities;
using Unity.NetCode;
using Natrium.Gameplay.Shared.Components;
using Natrium.Gameplay.Shared.Utilities;
using Natrium.Shared;

namespace Natrium.Gameplay.Server.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class AimSystem : SystemBase
    {
        protected override void OnCreate()
        {
            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(WorldUpdateAllocator);

            foreach (var (clientEntity, rpcAim, rpcEntity) in SystemAPI.Query<ReceiveRpcCommandRequest, RpcAim>().WithEntityAccess())
            {
                ecb.DestroyEntity(rpcEntity);

                if (rpcAim.MouseWorldPosition is { x: 0, y: 0 })
                {
                    Log.Error($"RpcAim.MouseWorldPosition: {rpcAim.MouseWorldPosition}");
                    continue;
                }

                var nid = EntityManager.GetComponentData<NetworkId>(clientEntity.SourceConnection);

                Log.Debug($"RpcAim from {clientEntity.SourceConnection}:{nid.Value} | MouseWorldPosition: {rpcAim.MouseWorldPosition}");

                var start = rpcAim.MouseWorldPosition;
                start.y = 10.0f; //ToDo: The plus 10 on y axis, comes from the offset of the camara
                var end = rpcAim.MouseWorldPosition;

                var entityPrefab = Utils.GetEntityPrefab(nid.Value, EntityManager);
                if (entityPrefab != Entity.Null)
                    ecb.AddComponent(entityPrefab, new RayCast { Start = start, End = end });
            }

            foreach (var (ro, goSrc, entity) in SystemAPI.Query<RayCastOutput, GhostOwner>().WithEntityAccess())
            {
                ecb.RemoveComponent<RayCastOutput>(entity);

                if (ro.Hit.Entity == Entity.Null)
                    Log.Warning($"Hited Entity {ro.Hit.Entity} is {ro.Hit.Entity}");

                var networkIDSource = goSrc.NetworkId;
                var networkIDTarget = 0;
                if (EntityManager.HasComponent<GhostOwner>(ro.Hit.Entity))
                    networkIDTarget = EntityManager.GetComponentData<GhostOwner>(ro.Hit.Entity).NetworkId;

                Log.Debug($"{entity}:{networkIDSource} hit {ro.Hit.Entity}:{networkIDTarget}");

                if (SystemAPI.HasComponent<PlayerName>(ro.Hit.Entity))
                {
                    var rpcEntity = ecb.CreateEntity();
                    ecb.AddComponent(rpcEntity, new RpcAttack
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
                    ecb.AddComponent(rpcEntity, new RpcTile
                    {
                        Start = ro.Start,
                        End = ro.End,
                        NetworkIdSource = networkIDSource
                    });
                    var entityConnection = Utils.GetEntityConnection(networkIDSource, EntityManager);

                    if (entityConnection != Entity.Null)
                        ecb.AddComponent(rpcEntity, new SendRpcCommandRequest { TargetConnection = entityConnection });
                }
            }

            ecb.Playback(EntityManager);
        }
    }
}