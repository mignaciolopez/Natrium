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
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation)]
    public partial struct ClassicMovementSystem : ISystem, ISystemStartStop
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
            
            foreach (var (position, inputAxis, localTransform, entity) 
                     in SystemAPI.Query<RefRW<Position>, RefRO<InputAxis>, RefRO<LocalTransform>>()
                         .WithAll<MoveClassicTag, Simulate>()
                         .WithDisabled<MoveTowardsTargetTag, OverlapBox>()
                         .WithEntityAccess()) //If its moving Ignore it
            {
                if (!state.EntityManager.IsComponentEnabled<MoveClassicTag>(entity))
                    Log.Error($".WithAll<MoveClassicTag");

                position.ValueRW.Target = math.round(localTransform.ValueRO.Position); 
                position.ValueRW.Previous = position.ValueRO.Target;
                
                //This bunch of Ifs Simulates the original Behavior of Movement.
                if (inputAxis.ValueRO.Value.y > 0)
                    position.ValueRW.Target.z++;
                else if (inputAxis.ValueRO.Value.x > 0)
                    position.ValueRW.Target.x++;
                else if (inputAxis.ValueRO.Value.y < 0)
                    position.ValueRW.Target.z--;
                else if (inputAxis.ValueRO.Value.x < 0)
                    position.ValueRW.Target.x--;

                if (inputAxis.ValueRO.Value.x != 0 || inputAxis.ValueRO.Value.y != 0)
                {
                    ecb.SetComponent(entity, new OverlapBox
                    {
                        HalfExtends = 0.2f,
                        Offset = new float3(0.2f * inputAxis.ValueRO.Value.x, 0, 0.2f * inputAxis.ValueRO.Value.y),
                    });
                    ecb.SetComponentEnabled<OverlapBox>(entity, true);
                    //ecb.SetComponentEnabled<MoveTowardsTargetTag>(entity, true);
                }
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}