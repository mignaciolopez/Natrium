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
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation)]
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
            var deltaTIme = SystemAPI.Time.DeltaTime;
            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);

            foreach (var (localTransform, position, speed, entity) 
                     in SystemAPI.Query<RefRW<LocalTransform>, RefRO<Position>, RefRO<Speed>>()
                         .WithAll<Simulate>()
                         .WithEntityAccess())
            {
                var maxDistanceDelta = speed.ValueRO.Value * deltaTIme;
                localTransform.ValueRW.Position.MoveTowards(position.ValueRO.Target, maxDistanceDelta);
                
                if (math.distancesq(localTransform.ValueRO.Position, position.ValueRO.Target) < maxDistanceDelta * 0.1f)
                {
                    ecb.SetComponentEnabled<MoveTowardsTargetTag>(entity, false);
                }
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}