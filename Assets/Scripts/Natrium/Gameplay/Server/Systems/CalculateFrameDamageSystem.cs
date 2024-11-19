using Natrium.Gameplay.Shared.Components;
using Natrium.Shared;
using Unity.Entities;
using Unity.NetCode;

namespace Natrium.Gameplay.Server.Systems
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderLast = true)]
    public partial struct CalculateFrameDamageSystem : ISystem, ISystemStartStop
    {
        //[BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] | {this.ToString()}.OnCreate()");
            state.RequireForUpdate<NetworkTime>();
        }

        //[BurstCompile]
        public void OnStartRunning(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] | {this.ToString()}.OnStartRunning()");
        }

        //[BurstCompile]
        public void OnStopRunning(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] | {this.ToString()}.OnStopRunning()");
        }

        //[BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] | {this.ToString()}.OnDestroy()");
        }

        public void OnUpdate(ref SystemState state)
        {
            var currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;

            foreach (var (dpb, dpt, e) in SystemAPI.Query<DynamicBuffer<DamagePointsBuffer>, DynamicBuffer<DamagePointsTick>>()
                .WithAll<Simulate>().WithEntityAccess())
            {
                if (currentTick.IsValid)
                {
                    if (dpb.IsEmpty)
                    {
                        dpt.AddCommandData(new DamagePointsTick { Tick = currentTick, Value = 0 });
                    }
                    else
                    {
                        var totalDamage = .0f;
                        if (dpt.GetDataAtTick(currentTick, out var damagePointsTick))
                        {
                            totalDamage = damagePointsTick.Value;
                        }

                        foreach (var dp in dpb)
                        {
                            totalDamage += dp.Value;
                        }

                        dpt.AddCommandData(new DamagePointsTick { Tick = currentTick, Value = totalDamage });

                        dpb.Clear();
                    }
                }
            }
        }
    }
}