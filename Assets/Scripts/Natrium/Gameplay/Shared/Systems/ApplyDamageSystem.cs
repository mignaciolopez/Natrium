using Natrium.Gameplay.Shared.Components;
using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;

namespace Natrium.Gameplay.Server.Systems
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderLast = true)]
    [UpdateAfter(typeof(CalculateFrameDamageSystem))]
    public partial struct ApplyDamageSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkTime>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (cHP, dpt, e) in SystemAPI.Query<RefRW<CurrentHealthPoints>, DynamicBuffer<DamagePointsTick>>()
                .WithAll<Simulate>().WithEntityAccess())
            {
                if (!dpt.GetDataAtTick(currentTick, out var damagePointsTick)) continue;
                if (damagePointsTick.Tick != currentTick) continue;
                cHP.ValueRW.Value -= damagePointsTick.Value;
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}