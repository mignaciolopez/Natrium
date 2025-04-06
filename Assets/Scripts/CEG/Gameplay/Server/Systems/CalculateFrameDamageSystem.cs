using CEG.Gameplay.Shared.Components;

using Unity.Entities;
using Unity.NetCode;

namespace CEG.Gameplay.Server.Systems
{
    [UpdateInGroup(typeof(AttackSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct CalculateFrameDamageSystem : ISystem, ISystemStartStop
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