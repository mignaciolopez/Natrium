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

        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
            var deltaTime = SystemAPI.Time.DeltaTime;
            
            foreach (var (position, inputAxis, speed, entity) 
                     in SystemAPI.Query<RefRW<Position>, RefRO<InputAxis>, RefRO<Speed>>()
                         .WithAll<MoveFreeTag, Simulate>()
                         .WithDisabled<MoveTowardsTargetTag, OverlapBox>()
                         .WithEntityAccess()) //If its moving Ignore it
            {
                if (!state.EntityManager.IsComponentEnabled<MoveFreeTag>(entity))
                    Log.Error($".WithAll<MoveFreeTag");
                
                position.ValueRW.Previous = position.ValueRO.Target;
                
                var input = new float3(inputAxis.ValueRO.Value.x, 0.0f, inputAxis.ValueRO.Value.y);
                position.ValueRW.Target += speed.ValueRO.Value * deltaTime * input;
                
                if (inputAxis.ValueRO.Value.x != 0 || inputAxis.ValueRO.Value.y != 0)
                {
                    if (state.EntityManager.HasComponent<GhostOwnerIsLocal>(entity))
                    {

                        ecb.SetComponent(entity, new OverlapBox
                        {
                            HalfExtends = 0.05f,
                            Offset = new float3(0.6f * math.round(inputAxis.ValueRO.Value.x), 0,
                                0.6f * math.round(inputAxis.ValueRO.Value.y)),
                        });
                        //ecb.SetComponentEnabled<OverlapBox>(entity, true); //ToDo: Resolve collision properly for FreeMovement
                    }
                    else
                    {
                        ecb.SetComponentEnabled<MoveTowardsTargetTag>(entity, true);   
                    }
                }
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}