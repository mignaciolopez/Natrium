using Unity.Entities;
using Unity.Physics;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Physics.Systems;

namespace Natrium
{
    public struct RaycastCommand : IComponentData
    {
        public Entity reqE;
        public float3 Start;
        public float3 End;
        public float MaxDistance;
    }

    public struct RaycastOutput : IComponentData
    {
        public Entity reqE;
        public float3 start;
        public float3 end;
        public Unity.Physics.RaycastHit hit;
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class RaycastSystem : SystemBase
    {
        private PhysicsWorldSingleton m_PhysicsWorldSingleton;
        private CollisionWorld m_CollisionWorld;

        protected override void OnCreate()
        {
            base.OnCreate();

            RequireForUpdate<RaycastCommand>();
            RequireForUpdate<PhysicsWorldSingleton>();
        }

        protected override void OnStartRunning()
        {
            var builder = new EntityQueryBuilder(Allocator.Temp).WithAll<Unity.Physics.PhysicsWorldSingleton>();
            EntityQuery singletonQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(builder);
            m_PhysicsWorldSingleton = singletonQuery.GetSingleton<Unity.Physics.PhysicsWorldSingleton>();
            m_CollisionWorld = m_PhysicsWorldSingleton.CollisionWorld;
            singletonQuery.Dispose();
        }

        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (rc, pc, e) in SystemAPI.Query<RaycastCommand, PhysicsCollider>().WithEntityAccess())
            {
                UnityEngine.Debug.Log($"'{World.Unmanaged.Name}' Entity {e} is RayCasting from {rc.Start} to {rc.End}");

                ecb.RemoveComponent<RaycastCommand>(e);

                Unity.Physics.RaycastInput input = new Unity.Physics.RaycastInput()
                {
                    Start = rc.Start,
                    End = rc.End,
                    Filter = new Unity.Physics.CollisionFilter()
                    {
                        BelongsTo = pc.Value.Value.GetCollisionFilter().BelongsTo,
                        CollidesWith = pc.Value.Value.GetCollisionFilter().CollidesWith,
                        GroupIndex = pc.Value.Value.GetCollisionFilter().GroupIndex
                    }
                };

                if (m_CollisionWorld.CastRay(input, out var hit))
                    ecb.AddComponent(e, new RaycastOutput { hit = hit, reqE = rc.reqE, start = rc.Start, end = rc.End });
            }

            ecb.Playback(EntityManager);
        }
    }
}