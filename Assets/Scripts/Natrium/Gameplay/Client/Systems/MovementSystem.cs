using Natrium.Gameplay.Shared.Components;
using Natrium.Gameplay.Shared.Components.Input;
using Natrium.Gameplay.Shared.Systems;
using Natrium.Shared;
using Natrium.Shared.Extensions;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

namespace Natrium.Gameplay.Client.Systems
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [UpdateBefore(typeof(PhysicsCastSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct MovementSystem : ISystem, ISystemStartStop
    {
        //[BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            Log.Verbose("OnCreate");
            state.RequireForUpdate<NetworkTime>(); 
        }

        //[BurstCompile]
        public void OnStartRunning(ref SystemState state)
        {
            Log.Verbose("OnStartRunning");
        }

        //[BurstCompile]
        public void OnStopRunning(ref SystemState state)
        {
            Log.Verbose("OnStopRunning");
        }

        //[BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            Log.Verbose("OnDestroy");
        }
        
        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
            
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();

            if (!networkTime.IsFirstTimeFullyPredictingTick)
                return;
            
            foreach (var (movement, position, inputMove, localTransform, reckoning)
                     in SystemAPI.Query<RefRW<MovementData>, DynamicBuffer<TargetCommand>, RefRO<InputMove>, RefRO<LocalTransform>, RefRO<Reckoning>>()
                         .WithAll<PredictedGhost, Simulate>())
            {
                if (movement.ValueRO.IsMoving)
                    continue;

                var target = (int3)math.round(localTransform.ValueRO.Position);
                
                if (inputMove.ValueRO.Value.y > 0)
                    target.z++;
                else if (inputMove.ValueRO.Value.x > 0)
                    target.x++;
                else if (inputMove.ValueRO.Value.y < 0)
                    target.z--;
                else if (inputMove.ValueRO.Value.x < 0)
                    target.x--;

                position.AddCommandData(new TargetCommand
                {
                    Tick = networkTime.ServerTick,
                    Target = target,
                });
                
                if (inputMove.ValueRO.Value.x != 0 || inputMove.ValueRO.Value.y != 0)
                {
                    movement.ValueRW.Target = target;

                    /*ecb.SetComponent(entity, new OverlapBox
                    {
                        HalfExtends = 0.4f,
                        Offset = float3.zero,
                    });
                    ecb.SetComponentEnabled<OverlapBox>(entity, true);*/
                }
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}