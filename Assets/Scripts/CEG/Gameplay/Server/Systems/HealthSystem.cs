using CEG.Gameplay.Shared.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace CEG.Gameplay.Server.Systems
{
    [UpdateInGroup(typeof(AttackSystemGroup))]
    [UpdateAfter(typeof(CalculateFrameDamageSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct HealthSystem : ISystem, ISystemStartStop
    {
        //[BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            Log.Verbose("OnCreate");
            state.RequireForUpdate<NetworkTime>();
        }

        //[BurstCompile]
        public void OnStartRunning(ref SystemState state)
        {
            Log.Verbose("OnStartRunning");
        }

        //[BurstCompile]
        public void OnStopRunning(ref SystemState state)
        {
            Log.Verbose("OnStopRunning");
        }

        //[BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            Log.Verbose("OnDestroy");
        }

        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (healthPoints, damagePointsAtTicks, e) 
                     in SystemAPI.Query<RefRW<HealthPoints>, DynamicBuffer<DamagePointsAtTick>>()
                         .WithAll<Simulate>().WithDisabled<DeathTag>().WithEntityAccess())
            {
                if (!damagePointsAtTicks.GetDataAtTick(currentTick, out var damagePointsAtTick)) continue;
                if (damagePointsAtTick.Tick != currentTick) continue;
                healthPoints.ValueRW.Value -= (int)damagePointsAtTick.Value;

                if (healthPoints.ValueRO.Value <= 0)
                {
                    healthPoints.ValueRW.Value = 0;
                    Log.Debug($"Setting DeathTag to True on {e}");
                    ecb.SetComponentEnabled<DeathTag>(e, true);
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}