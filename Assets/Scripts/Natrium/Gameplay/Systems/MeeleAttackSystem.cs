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
    public partial class MeeleAttackSystem : SystemBase
    {
        protected override void OnStartRunning()
        {
            EventSystem.Subscribe(Shared.Events.OnMeeleAttack, OnMeeleAttack);

            base.OnStartRunning();
        }

        protected override void OnStopRunning()
        {
            EventSystem.UnSubscribe(Shared.Events.OnMeeleAttack, OnMeeleAttack);

            base.OnStopRunning();
        }

        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }

        private void OnMeeleAttack(Shared.Stream stream)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }

    #endregion


    #region Server

    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class MeeleAttackSystemServer : SystemBase
    {
        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (clientEntity, rpcClick, rpcEntity) in SystemAPI.Query<ReceiveRpcCommandRequest, RpcAimAttack>().WithEntityAccess())
            {
                if (rpcClick.MouseWorldPosition is { x: 0, y: 0 })
                {
                    Debug.LogError($"RpcAimAttack.MouseWorldPosition: {rpcClick.MouseWorldPosition}");
                    continue;
                }

                Debug.Log($"'{World.Unmanaged.Name}' ReceiveRpcCommandRequest RpcAimAttack | MouseWorldPosition: {rpcClick.MouseWorldPosition}");

                //ghostEntity is the linked entity that contains all the prefab component, LocalTransform, Cube, Colliders, etc...
                var ghostEntity = EntityManager.GetBuffer<LinkedEntityGroup>(clientEntity.SourceConnection, true)[1].Value;

                

                ecb.DestroyEntity(rpcEntity);

            }

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }

    #endregion
}