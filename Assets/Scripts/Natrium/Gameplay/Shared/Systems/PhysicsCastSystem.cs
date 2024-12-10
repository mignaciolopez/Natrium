using Unity.Entities;
using Unity.Physics;
using Natrium.Gameplay.Shared.Components;
using Natrium.Gameplay.Shared.Components.Input;
using Natrium.Shared;
using Natrium.Shared.Extensions;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Unity.NetCode;

namespace Natrium.Gameplay.Shared.Systems
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [UpdateBefore(typeof(MoveTowardsTargetSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation)]
    public partial struct PhysicsCastSystem : ISystem, ISystemStartStop
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkTime>();
            Log.Verbose($"[{state.WorldUnmanaged.Name}] OnCreate");
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate<PhysicsWorldHistorySingleton>();
        }

        public void OnStartRunning(ref SystemState state)
        {
            //Log.Verbose($"[{state.WorldUnmanaged.Name}] OnStartRunning");
        }
        
        public void OnStopRunning(ref SystemState state)
        {
            //Log.Verbose($"[{state.WorldUnmanaged.Name}] OnStopRunning");
        }
        
        public void OnDestroy(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] OnDestroy");
        }
        
        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();

            if (state.WorldUnmanaged.IsClient() && !networkTime.IsFirstTimeFullyPredictingTick)
                return;
            
            var physicsWorld = SystemAPI.GetSingletonRW<PhysicsWorldSingleton>();
            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
            
            foreach (var (movementData, targetCommand, reckoning, physicsCollider, overlapBox, entity) 
                     in SystemAPI.Query<RefRW<MovementData>, DynamicBuffer<TargetCommand>, RefRW<Reckoning>, RefRO<PhysicsCollider>, RefRO<OverlapBox>>()
                         .WithAll<PredictedGhost, Simulate>()
                         .WithEntityAccess())
            {
                if (!targetCommand.GetDataAtTick(networkTime.ServerTick, out var targetCommandAtTick, false))
                {
                    //Log.Warning($"No {nameof(TargetCommand)}@{networkTime.ServerTick}");
                    continue;
                }
                
                if (movementData.ValueRO.IsMoving || !movementData.ValueRO.ShouldCheckCollision)
                    continue;
                
                var filter = physicsCollider.ValueRO.Value.Value.GetCollisionFilter();
                
                var outHits = new NativeList<DistanceHit>(state.WorldUpdateAllocator);

                if (state.WorldUnmanaged.IsServer())
                {
                    var interpolationDelay = networkTime.ServerTick;
                    interpolationDelay.Subtract(targetCommandAtTick.Tick.TickIndexForValidTick);
                    
                    SystemAPI.GetSingleton<PhysicsWorldHistorySingleton>().GetCollisionWorldFromTick(
                        targetCommandAtTick.Tick,
                        interpolationDelay.TickIndexForValidTick,
                        ref physicsWorld.ValueRW.PhysicsWorld, 
                        out collisionWorld );
                }
                
                var realCollision = collisionWorld.OverlapBox(
                    movementData.ValueRO.Target + overlapBox.ValueRO.Offset,
                    quaternion.identity,
                    overlapBox.ValueRO.HalfExtends,
                    ref outHits,
                    filter);
                
                if (realCollision)
                {
                    foreach (var hit in outHits)
                    {
                        if (hit.Entity == entity)
                        {
                            Log.Info($"[{state.World.Name}] {entity} is colliding with itself hit {hit.Entity}");
                            realCollision = false;
                        }
                        else
                        {
                            if (state.EntityManager.HasComponent<DeathTag>(hit.Entity) &&
                                state.EntityManager.IsComponentEnabled<DeathTag>(hit.Entity))
                                realCollision = false;
                            else
                            {
                                realCollision = true;
                            }
                        }
                    }
                }

                if (realCollision)
                {
                    movementData.ValueRW.Target = movementData.ValueRO.Previous;
                    //reckoning.ValueRW.Tick = networkTime.ServerTick;
                    //reckoning.ValueRW.ShouldReckon = true;
                    //reckoning.ValueRW.Target = movementData.ValueRO.Previous;
                }

                movementData.ValueRW.ShouldCheckCollision = false;
                
                outHits.Dispose();
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    [BurstCompile]
    public partial struct RayCastJob : IJobEntity
    {
        public EntityCommandBuffer ecb;
        public CollisionWorld collisionWorld;
        private void Execute(RefRO<RayCast> cr, RefRO<PhysicsCollider> pc, Entity e)
        {
            ecb.RemoveComponent<RayCast>(e);

            var input = new RaycastInput()
            {
                Start = cr.ValueRO.Start,
                End = cr.ValueRO.End,
                Filter = pc.ValueRO.Value.Value.GetCollisionFilter()
            };

            var isHit = collisionWorld.CastRay(input, out var hit);
            ecb.AddComponent(e, new RayCastOutput
            {
                IsHit = isHit,
                Hit = hit,
                Start = cr.ValueRO.Start,
                End = cr.ValueRO.End
            });
        }
    }

    [BurstCompile]
    public partial struct BoxCastJob : IJobEntity
    {
        public EntityCommandBuffer ecb;
        public CollisionWorld collisionWorld;
        private void Execute(RefRO<BoxCast> bc, RefRO<PhysicsCollider> pc, Entity e)
        {
            ecb.RemoveComponent<BoxCast>(e);

            var filter = pc.ValueRO.Value.Value.GetCollisionFilter();

            var isHit = collisionWorld.BoxCast(bc.ValueRO.Center, bc.ValueRO.Orientation, bc.ValueRO.HalfExtents, bc.ValueRO.Direction, bc.ValueRO.MaxDistance, out var hit, filter);
            ecb.AddComponent(e, new BoxCastOutput { IsHit = isHit, Hit = hit });
        }
    }
}