using Natrium.Gameplay.Shared.Components;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using Unity.Mathematics;

namespace Natrium.Gameplay.Server.Systems
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderLast = true)]
    public partial struct DestroyEntitySystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<NetworkTime>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();

            if (!networkTime.IsFirstTimeFullyPredictingTick)
                return;

            var ecbs = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbs.CreateCommandBuffer(state.WorldUnmanaged);

            foreach(var (lt, e) in SystemAPI.Query<RefRW<LocalTransform>>().WithAll<DestroyEntityTag, Simulate>()
                .WithEntityAccess())
            {
                if (state.World.IsServer())
                {
                    ecb.DestroyEntity(e);
                }
                else
                {
                    lt.ValueRW.Position = new float3(100000f, 100000f, 100000f);
                }
            }
        }
    }
}