using CEG.Extensions;
using CEG.Gameplay.Shared.Components;
using CEG.Gameplay.Shared.Components.Input;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

namespace CEG.Gameplay.Shared.Systems
{
    [UpdateInGroup(typeof(MovementSystemGroup))]
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

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();
            if (!networkTime.IsFirstTimeFullyPredictingTick)
                return;
            
            foreach (var (localTransform, movementData, moveCommand, speed) 
                     in SystemAPI.Query<RefRW<LocalTransform>, RefRW<MovementData>, DynamicBuffer<MoveCommand>, RefRO<Speed>>()
                         .WithAll<PredictedGhost, Simulate>())
            {
                if (!moveCommand.GetDataAtTick(networkTime.ServerTick, out var moveLastTick))
                    continue;
                
                if (math.distancesq(movementData.ValueRO.Previous, moveLastTick.Target) > 0.1f)
                    movementData.ValueRW.Direction =  math.normalize(moveLastTick.Target - movementData.ValueRO.Previous);
                
                localTransform.ValueRW.Rotation.RotateTowards(movementData.ValueRW.Direction, speed.ValueRO.Rotation);
            }
        }
    }
}