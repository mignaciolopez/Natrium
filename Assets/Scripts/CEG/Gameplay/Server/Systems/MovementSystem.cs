using CEG.Gameplay.Shared.Components;
using CEG.Gameplay.Shared.Components.Input;
using CEG.Gameplay.Shared.Systems;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

namespace CEG.Gameplay.Server.Systems
{
    [UpdateInGroup(typeof(MovementSystemGroup))]
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
            
            foreach (var (movementData, position, reckoning, localTransform, entity)
                     in SystemAPI.Query<RefRW<MovementData>, DynamicBuffer<MoveCommand>, RefRW<Reckoning>, RefRW<LocalTransform>>()
                         .WithAll<PredictedGhost, Simulate>()
                         .WithEntityAccess())
            {
                if (!position.GetDataAtTick(networkTime.ServerTick, out var positionAtTick))
                {
                    //Log.Warning($"No {nameof(TargetCommand)}@{networkTime.ServerTick}");
                    continue;
                }

                if (movementData.ValueRO.IsMoving)
                    continue;
                
                
                var distance = math.distancesq(localTransform.ValueRO.Position, positionAtTick.Target);
                movementData.ValueRW.ShouldCheckCollision = distance > 0.1f && !state.EntityManager.IsComponentEnabled<DeathTag>(entity);
                
                if (distance > 2.0f)
                {
                    movementData.ValueRW.Target = movementData.ValueRO.Previous;
                    
                    reckoning.ValueRW.Tick = networkTime.ServerTick;
                    reckoning.ValueRW.ShouldReckon = true;
                    reckoning.ValueRW.Target = movementData.ValueRO.Previous;
                }
                else
                {
                    movementData.ValueRW.Target = positionAtTick.Target;
                    movementData.ValueRW.Previous = math.round(localTransform.ValueRO.Position);
                    movementData.ValueRW.Previous.y = localTransform.ValueRO.Position.y;
                    reckoning.ValueRW.ShouldReckon = false;
                }
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}