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
            //Log.Debug($"[{state.WorldUnmanaged.Name}] ServerTick: {networkTime.ServerTick} ");
            //Log.Debug($"[{state.WorldUnmanaged.Name}] InterpolationTick: {networkTime.InterpolationTick} ");
            
            /*var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
            
            foreach (var (position, inputMove, localTransform, entity) 
                     in SystemAPI.Query<RefRW<MoveCommands>, DynamicBuffer<InputMove>, RefRO<LocalTransform>>()
                         .WithAll<MoveClassicTag, Simulate>()
                         .WithDisabled<MovingTowardsTargetTag, OverlapBox>()
                         .WithEntityAccess()) //If its moving Ignore it
            {
                if (!state.EntityManager.IsComponentEnabled<MoveClassicTag>(entity))
                    Log.Error($".WithAll<MoveClassicTag");

                inputMove.GetDataAtTick(networkTime.ServerTick, out var inputMoveAtTick);

                position.ValueRW.Target = math.round(localTransform.ValueRO.Position); 
                position.ValueRW.Previous = position.ValueRO.Target;
                
                //This bunch of Ifs Simulates the original Behavior of Movement.
                if (inputMoveAtTick.Value.y > 0)
                    position.ValueRW.Target.z++;
                else if (inputMoveAtTick.Value.x > 0)
                    position.ValueRW.Target.x++;
                else if (inputMoveAtTick.Value.y < 0)
                    position.ValueRW.Target.z--;
                else if (inputMoveAtTick.Value.x < 0)
                    position.ValueRW.Target.x--;

                if (inputMoveAtTick.Value.x != 0 || inputMoveAtTick.Value.y != 0)
                {
                    if (state.EntityManager.HasComponent<GhostOwnerIsLocal>(entity))
                    {
                        ecb.SetComponent(entity, new OverlapBox
                        {
                            Tick = inputMoveAtTick.Tick,
                            HalfExtends = 0.4f,
                            Offset = new float3(0.1f * inputMoveAtTick.Value.x, 0, 0.1f * inputMoveAtTick.Value.y),
                        });
                        //ecb.SetComponentEnabled<OverlapBox>(entity, true);
                        ecb.SetComponentEnabled<MovingTowardsTargetTag>(entity, true); //Overriding collision check
                    }
                    else
                    {
                        ecb.SetComponentEnabled<MovingTowardsTargetTag>(entity, true);
                    }
                }
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
            */
        }
    }
}