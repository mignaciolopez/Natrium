using Natrium.Gameplay.Shared.Components;
using Natrium.Shared;
using Natrium.Shared.Extensions;
using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;
using Unity.Physics;
using Unity.Transforms;

namespace Natrium.Gameplay.Server.Systems
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderLast = true)]
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
            var currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);

            foreach (var (speed,
                         physicsCollider, hp,dt, rt, entity)
                     in SystemAPI.Query<RefRW<Speed>, RefRW<PhysicsCollider>,
                        RefRW<HealthPoints>, EnabledRefRW<DeathTag>, EnabledRefRW<ResurrectTag>>()
                         .WithEntityAccess())
            {
                Log.Debug($"Resurrecting {entity}");

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
                
                speed.ValueRW.Value /= 2.0f;
                
                ecb.SetComponentEnabled<MoveTowardsTag>(entity, false);
                ecb.SetComponentEnabled<OverlapBox>(entity, false);
                ecb.SetComponentEnabled<MoveFreeTag>(entity, false);
                ecb.SetComponentEnabled<MoveClassicTag>(entity, true);
                
                ecb.SetComponentEnabled<Attack>(entity, true);
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
    }
    
}