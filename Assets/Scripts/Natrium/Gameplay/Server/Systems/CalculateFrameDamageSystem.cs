using Natrium.Gameplay.Shared.Components;
using Natrium.Shared;
using Unity.Entities;
using Unity.NetCode;

namespace Natrium.Gameplay.Server.Systems
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderLast = true)]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
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

            if (!currentTick.IsValid)
                return;
            
            foreach (var (damagePointsBuffer, damagePointsAtTicks) in 
                     SystemAPI.Query<DynamicBuffer<DamagePointsBuffer>, DynamicBuffer<DamagePointsAtTick>>()
                         .WithAll<Simulate>())
            {
                if (damagePointsBuffer.IsEmpty)
                {
                    damagePointsAtTicks.AddCommandData(new DamagePointsAtTick
                    {
                        Tick = currentTick,
                        Value = 0
                    });
                }
                else
                {
                    var totalDamage = .0f;
                    if (damagePointsAtTicks.GetDataAtTick(currentTick, out var damagePointAtTick))
                    {
                        totalDamage = damagePointAtTick.Value;
                    }

                    foreach (var dp in damagePointsBuffer)
                    {
                        totalDamage += dp.Value;
                    }

                    damagePointsAtTicks.AddCommandData(new DamagePointsAtTick { Tick = currentTick, Value = totalDamage });

                    damagePointsBuffer.Clear();
                }
            }
        }
    }
}