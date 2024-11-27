using Natrium.Gameplay.Shared.Components;
using Natrium.Gameplay.Shared.Components.Input;
using Natrium.Shared;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

namespace Natrium.Gameplay.Shared.Systems
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [UpdateBefore(typeof(PhysicsCastSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial struct FreeMovementSystem : ISystem, ISystemStartStop
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

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
            var deltaTime = SystemAPI.Time.DeltaTime;
            
            foreach (var (position,localTransform, inputAxis, speed, entity) 
                     in SystemAPI.Query<RefRW<Position>, RefRW<LocalTransform>, RefRO<InputAxis>, RefRO<Speed>>()
                         .WithAll<MoveFreeTag, Simulate>()
                         .WithDisabled<MoveTowardsTag, MoveClassicTag, MoveDiagonalTag>()
                         .WithEntityAccess()) //If its moving Ignore it
            {
                if (!state.EntityManager.IsComponentEnabled<MoveFreeTag>(entity))
                {
                    Log.Error($"[{state.World.Name}] .WithAll<MoveFreeTag> is accounting for disabled MoveFreeTag");
                }
                
                position.ValueRW.Target = math.round(localTransform.ValueRO.Position);
                position.ValueRW.Previous = position.ValueRO.Target;
                
                var input = new float3(inputAxis.ValueRO.Value.x, 0.0f, inputAxis.ValueRO.Value.y);
                localTransform.ValueRW.Position += speed.ValueRO.Value * deltaTime * input;
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}