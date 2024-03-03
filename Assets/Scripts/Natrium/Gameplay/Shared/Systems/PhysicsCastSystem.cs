using Unity.Entities;
using Unity.Physics;
using Natrium.Gameplay.Shared.Components;
using Unity.Burst;

namespace Natrium.Gameplay.Shared.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial struct PhysicsCastSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<RaycastSystemExecute>();
            state.RequireForUpdate<PhysicsWorldSingleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecbs = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();

            new RayCastJob
            {
                ecb = ecbs.CreateCommandBuffer(state.WorldUnmanaged),
                collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld
            }.Schedule();

            new BoxCastJob
            {
                ecb = ecbs.CreateCommandBuffer(state.WorldUnmanaged),
                collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld
            }.Schedule();
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

            if (collisionWorld.CastRay(input, out var hit))
            {
                ecb.AddComponent(e, new RayCastOutput { Hit = hit, Start = cr.ValueRO.Start, End = cr.ValueRO.End });
            }
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

            if (collisionWorld.BoxCast(bc.ValueRO.Center, bc.ValueRO.Orientation, bc.ValueRO.HalfExtents, bc.ValueRO.Direction, bc.ValueRO.MaxDistance, out var hit, filter))
            {
                ecb.AddComponent(e, new BoxCastOutput { Hit = hit });
            }
        }
    }
}