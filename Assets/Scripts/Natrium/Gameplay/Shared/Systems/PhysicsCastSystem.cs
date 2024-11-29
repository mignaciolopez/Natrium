using Unity.Entities;
using Unity.Physics;
using Natrium.Gameplay.Shared.Components;
using Natrium.Shared;
using Unity.Burst;
using Unity.Collections;
using Unity.NetCode;
using Unity.Transforms;

namespace Natrium.Gameplay.Shared.Systems
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [UpdateBefore(typeof(MoveTowardsTargetSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial struct PhysicsCastSystem : ISystem, ISystemStartStop
    {
        public void OnCreate(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] OnCreate");
            state.RequireForUpdate<PhysicsWorldSingleton>();
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
            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;

            foreach (var (position, localTransform, physicsCollider, overlapBox, entity) in
                     SystemAPI.Query<RefRW<Position>, RefRO<LocalTransform>, RefRO<PhysicsCollider>, RefRO<OverlapBox>>()
                         .WithDisabled<MoveTowardsTargetTag>() //Ignore Already Moving Entities
                         .WithAll<OverlapBox, Simulate>()
                         .WithEntityAccess())
            {
                var filter = physicsCollider.ValueRO.Value.Value.GetCollisionFilter();

                var outHits = new NativeList<DistanceHit>(state.WorldUpdateAllocator);

                var realCollision = collisionWorld.OverlapBox(
                    position.ValueRO.Target + overlapBox.ValueRO.Offset,
                    localTransform.ValueRO.Rotation,
                    overlapBox.ValueRO.HalfExtends,
                    ref outHits,
                    filter);

                if (realCollision)
                {
                    foreach (var hit in outHits)
                    {
                        if (hit.Entity == entity)
                        {
                            Log.Warning($"[{state.World.Name}] {entity} is colliding with itself hit {hit.Entity}");
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
                    position.ValueRW.Target = position.ValueRO.Previous;
                }
                else
                {
                    ecb.SetComponentEnabled<MoveTowardsTargetTag>(entity, true);    
                }

                ecb.SetComponentEnabled<OverlapBox>(entity, false);
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