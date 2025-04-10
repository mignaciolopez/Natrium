using CEG.Gameplay.Shared.Components;
using CEG.Extensions;
using Unity.Entities;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;

namespace CEG.Gameplay.Server.Systems
{
    [UpdateInGroup(typeof(AttackSystemGroup))]
    [UpdateAfter(typeof(DeathSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct ResurrectionSystem : ISystem, ISystemStartStop
    {
        private EndSimulationEntityCommandBufferSystem.Singleton _esEcbS;
        
        //[BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            Log.Verbose("OnCreate");
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<NetworkTime>();
        }

        //[BurstCompile]
        public void OnStartRunning(ref SystemState state)
        {
            Log.Verbose("OnStartRunning");
            _esEcbS = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
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
            Resurrect(ref state);
            
            var serverTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);

            foreach (var (speed, physicsCollider, hp, entity)
                     in SystemAPI.Query<RefRW<Speed>, RefRW<PhysicsCollider>, RefRW<HealthPoints>>()
                         .WithAll<DeathTag, ResurrectTag>().WithEntityAccess())
            {
                Log.Debug($"Resurrecting {entity}@{serverTick}");

                foreach (var child in state.EntityManager.GetBuffer<Child>(entity))
                {
                    if (!state.EntityManager.HasComponent<MaterialPropertyBaseColor>(child.Value))
                        continue;

                    var a = state.EntityManager.GetComponentData<MaterialPropertyBaseColor>(child.Value);
                    var b = state.EntityManager.GetComponentData<ColorAlive>(child.Value);
                    a.Value = b.Value.ToFloat4();
                    state.EntityManager.SetComponentData(child.Value, a);
                }
                
                hp.ValueRW.Value = hp.ValueRO.MaxValue;
                
                speed.ValueRW.Translation /= 2.0f;
                
                var collisionFilter = physicsCollider.ValueRO.Value.Value.GetCollisionFilter();
                collisionFilter.CollidesWith = ~0u;
                physicsCollider.ValueRW.Value.Value.SetCollisionFilter(collisionFilter);
                
                Log.Debug($"Setting DeathTag to false on {entity}");
                ecb.SetComponentEnabled<DeathTag>(entity, false);
                Log.Debug($"Setting ResurrectTag to false on {entity}");
                ecb.SetComponentEnabled<ResurrectTag>(entity, false);
                
                ecb.RemoveComponent<DeathInitialized>(entity);
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
        
        private void Resurrect(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
            const float range = 2.0f;
            
            foreach (var (ltw, dt, e) in SystemAPI.Query<LocalToWorld, EnabledRefRO<DeathTag>>()
                         .WithDisabled<ResurrectTag>().WithEntityAccess())
            {
                if (ltw.Position.x > -range && ltw.Position.x < range)
                {
                    if (ltw.Position.z > -range && ltw.Position.z < range)
                    {
                        Log.Debug($"Setting ResurrectTag to true on {e}");
                        ecb.SetComponentEnabled<ResurrectTag>(e, true);
                    }
                }
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
    
}