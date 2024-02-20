using Unity.Entities;
using Unity.Physics;
using Unity.Collections;
using Natrium.Gameplay.Server.Components;
using Natrium.Gameplay.Shared.Components;
using Natrium.Shared;

namespace Natrium.Gameplay.Server.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class RaycastSystem : SystemBase
    {
        private PhysicsWorldSingleton _physicsWorldSingleton;
        private CollisionWorld _collisionWorld;

        protected override void OnCreate()
        {
            base.OnCreate();

            RequireForUpdate<RaycastSystemExecute>();
            RequireForUpdate<RaycastCommand>();
            RequireForUpdate<PhysicsWorldSingleton>();
        }

        protected override void OnStartRunning()
        {
            var builder = new EntityQueryBuilder(Allocator.Temp).WithAll<PhysicsWorldSingleton>();
            var singletonQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(builder);
            _physicsWorldSingleton = singletonQuery.GetSingleton<PhysicsWorldSingleton>();
            _collisionWorld = _physicsWorldSingleton.CollisionWorld;
            singletonQuery.Dispose();
        }

        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (rc, pc, entity) in SystemAPI.Query<RaycastCommand, PhysicsCollider>().WithEntityAccess())
            {
                Log.Debug($"'{World.Name}' Entity {entity} is RayCasting from {rc.Start} to {rc.End}");

                ecb.RemoveComponent<RaycastCommand>(entity);

                var input = new RaycastInput()
                {
                    Start = rc.Start,
                    End = rc.End,
                    Filter = new CollisionFilter()
                    {
                        BelongsTo = pc.Value.Value.GetCollisionFilter().BelongsTo,
                        CollidesWith = pc.Value.Value.GetCollisionFilter().CollidesWith,
                        GroupIndex = pc.Value.Value.GetCollisionFilter().GroupIndex
                    }
                };

                if (_collisionWorld.CastRay(input, out var hit))
                {
                    Log.Debug($"'{World.Name}' {entity} Hit {hit.Entity}");
                    ecb.AddComponent(entity, new RaycastOutput { Hit = hit, Start = rc.Start, End = rc.End });
                }
                else
                    Log.Debug($"'{World.Name}' Entity {entity} No Hit!");
            }

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}