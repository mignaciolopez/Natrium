using Natrium.Gameplay.Shared.Components;
using Natrium.Gameplay.Shared.Components.Input;
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
    [UpdateAfter(typeof(MoveTowardsTargetSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial struct RotateTowardsTargetSystem : ISystem
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

        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTIme = SystemAPI.Time.DeltaTime;

            var networkTime = SystemAPI.GetSingleton<NetworkTime>();
            
            if (!networkTime.IsFirstTimeFullyPredictingTick)
                return;
            
            foreach (var (localTransform, movementData, moveCommand, speed) 
                     in SystemAPI.Query<RefRW<LocalTransform>, RefRW<MovementData>, DynamicBuffer<MoveCommand>, RefRO<Speed>>()
                         .WithAll<PredictedGhost, Simulate>())
            {
                if (!moveCommand.GetDataAtTick(networkTime.ServerTick, out var moveLastTick))
                    continue;
                
                var maxRotationDelta = speed.ValueRO.Rotation;
                
                
                if (math.distancesq(movementData.ValueRO.Previous, moveLastTick.Target) > 0.1f)
                    movementData.ValueRW.Direction =  moveLastTick.Target - movementData.ValueRO.Previous;
                
                localTransform.ValueRW.Rotation.RotateTowards(math.normalize(movementData.ValueRW.Direction), maxRotationDelta);
            }
        }
    }
}