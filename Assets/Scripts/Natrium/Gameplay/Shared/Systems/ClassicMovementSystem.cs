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

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
            
            foreach (var (position,localTransform, inputAxis, entity) 
                     in SystemAPI.Query<RefRW<Position>, RefRO<LocalTransform>, RefRO<InputAxis>>()
                         .WithAll<MoveClassicTag>()
                         .WithNone<MoveTowardsTag>()
                         .WithEntityAccess()) //If its moving Ignore it
            {
                position.ValueRW.Target = math.round(localTransform.ValueRO.Position);
                position.ValueRW.Previous = position.ValueRO.Target;
                
                //This bunch of Ifs Simulates the original Behavior of Processing Movement Inpupt.
                if (inputAxis.ValueRO.Value.y > 0)
                    position.ValueRW.Target.z++;
                else if (inputAxis.ValueRO.Value.x > 0)
                    position.ValueRW.Target.x++;
                else if (inputAxis.ValueRO.Value.y < 0)
                    position.ValueRW.Target.z--;
                else if (inputAxis.ValueRO.Value.x < 0)
                    position.ValueRW.Target.x--;

                if (position.ValueRO.Previous.x != position.ValueRO.Target.x ||
                    position.ValueRO.Previous.z != position.ValueRO.Target.z)
                {
                    ecb.AddComponent<OverlapBoxTag>(entity);    
                }
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}