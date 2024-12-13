using Natrium.Gameplay.Shared.Components;
using Natrium.Shared;
using Natrium.Shared.Extensions;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine.Analytics;
using UnityEngine.SocialPlatforms;

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
            
            foreach (var (localTransform, movementData, speed, child) 
                     in SystemAPI.Query<RefRW<LocalTransform>, RefRW<MovementData>, RefRO<Speed>, DynamicBuffer<Child>>()
                         .WithAll<PredictedGhost, Simulate>())
            {
                if(movementData.ValueRO.ShouldCheckCollision)
                    continue;
                
                var maxDistanceDelta = speed.ValueRO.Value * deltaTIme;
                localTransform.ValueRW.Position.MoveTowards(movementData.ValueRO.Target, maxDistanceDelta);
                
                if (movementData.ValueRO.IsMoving)
                    localTransform.ValueRW.Rotation.RotateTowards(movementData.ValueRO.Previous, movementData.ValueRO.Target, 360f);
                
                if (math.distancesq(localTransform.ValueRO.Position, movementData.ValueRO.Target) < maxDistanceDelta * 0.1f)
                {
                    movementData.ValueRW.IsMoving = false;
                    movementData.ValueRW.Previous = movementData.ValueRO.Target;
                }
                else
                {
                    movementData.ValueRW.IsMoving = true;
                }
            }
        }
    }
}