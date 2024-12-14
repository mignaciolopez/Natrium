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
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();
            
            if (!networkTime.IsFirstTimeFullyPredictingTick)
                return;
            
            foreach (var (movementData, moveCommand, speed, child) 
                     in SystemAPI.Query<RefRW<MovementData>, DynamicBuffer<MoveCommand>, RefRO<Speed>, DynamicBuffer<Child>>()
                         .WithAll<PredictedGhost, Simulate>())
            {
                if (!moveCommand.GetDataAtTick(networkTime.ServerTick, out var moveLastTick))
                    continue;
                
                var maxRotationDelta = speed.ValueRO.Rotation;

                if (math.distancesq(movementData.ValueRO.Previous, moveLastTick.Target) > 0.1f)
                    movementData.ValueRW.Direction =  moveLastTick.Target - movementData.ValueRO.Previous;
                
                foreach (var childEntity in child)
                {
                    if (!state.EntityManager.HasComponent<MaterialPropertyBaseColor>(childEntity.Value))
                        continue;

                    var childLocalTransform = state.EntityManager.GetComponentData<LocalTransform>(childEntity.Value);
                    childLocalTransform.Rotation.RotateTowards(math.normalize(movementData.ValueRW.Direction), maxRotationDelta);
                    state.EntityManager.SetComponentData(childEntity.Value, childLocalTransform);
                    break;
                }
            }
        }
    }
}