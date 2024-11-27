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
    public partial struct DiagonalMovementSystem : ISystem, ISystemStartStop
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
            
            foreach (var (position,localTransform, inputAxis, entity) 
                     in SystemAPI.Query<RefRW<Position>, RefRO<LocalTransform>, RefRO<InputAxis>>()
                         .WithAll<MoveDiagonalTag, Simulate>()
                         .WithNone<MoveClassicTag, MoveFreeTag>()
                         .WithDisabled<MoveTowardsTag>()
                         .WithEntityAccess()) //If its moving Ignore it
            {
                position.ValueRW.Target = math.round(localTransform.ValueRO.Position);
                position.ValueRW.Previous = position.ValueRO.Target;
                
                switch (inputAxis.ValueRO.Value.x)
                {
                    case > 0:
                        position.ValueRW.Target.x++;
                        break;
                    case < 0:
                        position.ValueRW.Target.x--;
                        break;
                }

                switch (inputAxis.ValueRO.Value.y)
                {
                    case > 0:
                        position.ValueRW.Target.z++;
                        break;
                    case < 0:
                        position.ValueRW.Target.z--;
                        break;
                }

                if (position.ValueRO.Previous.x != position.ValueRO.Target.x ||
                    position.ValueRO.Previous.z != position.ValueRO.Target.z)
                {
                    ecb.SetComponent(entity, new OverlapBox
                    {
                        HalfExtends = 0.49f,
                        Offset = float3.zero,
                    });
                    ecb.SetComponentEnabled<OverlapBox>(entity, true);
                }
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}