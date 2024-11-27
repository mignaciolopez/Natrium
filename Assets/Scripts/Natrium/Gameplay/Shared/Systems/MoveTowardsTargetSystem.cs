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
            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);

            foreach (var (localTransform, position, speed, entity) 
                     in SystemAPI.Query<RefRW<LocalTransform>, RefRO<Position>, RefRO<Speed>>()
                         .WithAll<MoveTowardsTag>()
                         .WithEntityAccess())
            {
                if (!state.EntityManager.IsComponentEnabled<MoveTowardsTag>(entity))
                {
                    Log.Error($"[{state.World.Name}] .WithAll<MoveTowardsTag>() is accounting for disabled MoveTowardsTag");
                }
                
                var maxDistanceDelta = speed.ValueRO.Value * SystemAPI.Time.DeltaTime;
                localTransform.ValueRW.Position.MoveTowards(position.ValueRO.Target, maxDistanceDelta);
                
                if (math.distance(localTransform.ValueRO.Position, position.ValueRO.Target) <= 0.0f)
                {
                    ecb.SetComponentEnabled<MoveTowardsTag>(entity, false);
                }
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}