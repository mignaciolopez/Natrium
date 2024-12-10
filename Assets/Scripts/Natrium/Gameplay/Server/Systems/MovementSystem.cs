using Natrium.Gameplay.Shared.Components;
using Natrium.Gameplay.Shared.Components.Input;
using Natrium.Gameplay.Shared.Systems;
using Natrium.Shared;
using Natrium.Shared.Extensions;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

namespace Natrium.Gameplay.Server.Systems
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [UpdateBefore(typeof(PhysicsCastSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
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
            
            foreach (var (movement, position, reckoning, localTransform)
                     in SystemAPI.Query<RefRW<MovementData>, DynamicBuffer<MoveCommand>, RefRW<Reckoning>, RefRO<LocalTransform>>()
                         .WithAll<PredictedGhost, Simulate>())
            {
                if (!position.GetDataAtTick(networkTime.ServerTick, out var positionAtTick))
                {
                    //Log.Warning($"No {nameof(TargetCommand)}@{networkTime.ServerTick}");
                    continue;
                }

                if (movement.ValueRO.IsMoving)
                    continue;
                
                
                var distance = math.distancesq(localTransform.ValueRO.Position, positionAtTick.Target);
                movement.ValueRW.ShouldCheckCollision = distance > 0.1f;
                
                if (distance > 2.0f)
                {
                    movement.ValueRW.Target = movement.ValueRO.Previous;
                    
                    reckoning.ValueRW.Tick = networkTime.ServerTick;
                    reckoning.ValueRW.ShouldReckon = true;
                    reckoning.ValueRW.Target = movement.ValueRO.Previous;
                }
                else
                {
                    movement.ValueRW.Target = positionAtTick.Target;
                    movement.ValueRW.Previous = (int3)math.round(localTransform.ValueRO.Position);
                    reckoning.ValueRW.ShouldReckon = false;
                }
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}