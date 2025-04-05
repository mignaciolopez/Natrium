using CEG.Gameplay.Shared.Components;
using CEG.Shared;
using CEG.Shared.Extensions;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

namespace CEG.Gameplay.Shared.Systems
{
    [UpdateInGroup(typeof(MovementSystemGroup))]
    [UpdateAfter(typeof(PhysicsCastSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial struct MoveTowardsTargetSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] OnCreate");
            state.RequireForUpdate<NetworkTime>();
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

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTIme = SystemAPI.Time.DeltaTime;
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();
            if (!networkTime.IsFirstTimeFullyPredictingTick)
                return;
            
            foreach (var (localTransform, movementData, speed) 
                     in SystemAPI.Query<RefRW<LocalTransform>, RefRW<MovementData>, RefRO<Speed>>()
                         .WithAll<PredictedGhost, Simulate>())
            {
                if(movementData.ValueRO.ShouldCheckCollision)
                    continue;
                
                var maxDistanceDelta = speed.ValueRO.Translation * deltaTIme;
                localTransform.ValueRW.Position.MoveTowards(movementData.ValueRO.Target, maxDistanceDelta);
                
                if (math.distancesq(localTransform.ValueRO.Position, movementData.ValueRO.Target) < maxDistanceDelta * movementData.ValueRO.PercentNextMove)
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