using Natrium.Gameplay.Shared.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace Natrium.Gameplay.Server.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class MeeleSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (clientEntity, rpcClick, rpcEntity) in SystemAPI.Query<ReceiveRpcCommandRequest, RpcAim>().WithEntityAccess())
            {
                if (rpcClick.MouseWorldPosition is { x: 0, y: 0 })
                {
                    UnityEngine.Debug.LogError($"RpcAimAttack.MouseWorldPosition: {rpcClick.MouseWorldPosition}");
                    continue;
                }

                UnityEngine.Debug.Log($"'{World.Unmanaged.Name}' ReceiveRpcCommandRequest RpcAimAttack | MouseWorldPosition: {rpcClick.MouseWorldPosition}");

                //ghostEntity is the linked entity that contains all the prefab component, LocalTransform, Cube, Colliders, etc...
                var ghostEntity = EntityManager.GetBuffer<LinkedEntityGroup>(clientEntity.SourceConnection, true)[1].Value;



                ecb.DestroyEntity(rpcEntity);

            }

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}
