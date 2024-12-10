using Natrium.Gameplay.Shared.Components;
using Natrium.Shared;
using Natrium.Shared.Extensions;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

namespace Natrium.Gameplay.Shared.Systems
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial struct MoveTowardsTargetSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] OnCreate");
        }

        public void OnStartRunning(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] OnStartRunning");
        }
        
        public void OnStopRunning(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] OnStopRunning");
        }
        
        public void OnDestroy(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] OnDestroy");
        }

        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTIme = SystemAPI.Time.DeltaTime;

            var networkTime = SystemAPI.GetSingleton<NetworkTime>();
            
            if (!networkTime.IsFirstTimeFullyPredictingTick)
                return;
            
            foreach (var (localTransform, movement, speed) 
                     in SystemAPI.Query<RefRW<LocalTransform>, RefRW<MovementData>, RefRO<Speed>>()
                         .WithAll<PredictedGhost, Simulate>())
            {
                var maxDistanceDelta = speed.ValueRO.Value * deltaTIme;
                localTransform.ValueRW.Position.MoveTowards(movement.ValueRO.Target, maxDistanceDelta);
                
                if (math.distancesq(localTransform.ValueRO.Position, movement.ValueRO.Target) < maxDistanceDelta * 0.1f)
                {
                    movement.ValueRW.IsMoving = false;
                    movement.ValueRW.Previous = movement.ValueRO.Target;
                }
                else
                {
                    movement.ValueRW.IsMoving = true;
                }
            }
        }
    }
}