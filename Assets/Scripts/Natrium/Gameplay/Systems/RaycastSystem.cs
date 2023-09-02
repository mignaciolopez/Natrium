using Unity.Entities;
using Unity.Physics;
using Unity.Collections;
using Natrium.Gameplay.Components;

namespace Natrium.Gameplay.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class RaycastSystem : SystemBase
    {
        private PhysicsWorldSingleton _physicsWorldSingleton;
        private CollisionWorld _collisionWorld;

        protected override void OnCreate()
        {
            base.OnCreate();

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

            foreach (var (rc, pc, e) in SystemAPI.Query<RaycastCommand, PhysicsCollider>().WithEntityAccess())
            {
                UnityEngine.Debug.Log($"'{World.Unmanaged.Name}' Entity {e} is RayCasting from {rc.Start} to {rc.End}");

                ecb.RemoveComponent<RaycastCommand>(e);

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
                    ecb.AddComponent(e, new RaycastOutput { Hit = hit, ReqE = rc.ReqE, Start = rc.Start, End = rc.End });
            }

            ecb.Playback(EntityManager);
        }
    }
}