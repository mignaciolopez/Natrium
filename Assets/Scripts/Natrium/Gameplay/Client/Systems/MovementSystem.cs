using Natrium.Gameplay.Shared.Components;
using Natrium.Gameplay.Shared.Components.Input;
using Natrium.Gameplay.Shared.Systems;
using Natrium.Shared;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

namespace Natrium.Gameplay.Client.Systems
{
    [UpdateInGroup(typeof(MovementSystemGroup))]
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
            
            foreach (var (movementData, position, inputMove, localTransform, entity)
                     in SystemAPI.Query<RefRW<MovementData>, DynamicBuffer<MoveCommand>, RefRO<InputMove>, RefRW<LocalTransform>>()
                         .WithAll<PredictedGhost, Simulate>()
                         .WithEntityAccess())
            {
                if (movementData.ValueRO.IsMoving)
                    continue;

                var target = math.round(localTransform.ValueRO.Position);
                target.y = localTransform.ValueRO.Position.y;
                
                if (inputMove.ValueRO.Value.y > 0)
                    target.z++;
                else if (inputMove.ValueRO.Value.x > 0)
                    target.x++;
                else if (inputMove.ValueRO.Value.y < 0)
                    target.z--;
                else if (inputMove.ValueRO.Value.x < 0)
                    target.x--;
                
                position.AddCommandData(new MoveCommand
                {
                    Tick = networkTime.ServerTick,
                    Target = target,
                });
                
                movementData.ValueRW.ShouldCheckCollision = false;
                
                if (inputMove.ValueRO.Value.x != 0 || inputMove.ValueRO.Value.y != 0)
                {
                    movementData.ValueRW.Target = target;
                    movementData.ValueRW.ShouldCheckCollision = !state.EntityManager.IsComponentEnabled<DeathTag>(entity);
                }
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}