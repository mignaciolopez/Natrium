using Natrium.Gameplay.Shared.Components;
using Natrium.Shared;
using Natrium.Shared.Extensions;
using Unity.Entities;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;

namespace Natrium.Gameplay.Server.Systems
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderLast = true)]
    [UpdateAfter(typeof(HealthSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct DeathSystem : ISystem, ISystemStartStop
    {
        //[BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            Log.Verbose("OnCreate");
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

            foreach (var (speed, physicsCollider, entity)
                     in SystemAPI.Query<RefRW<Speed>,
                             RefRW<PhysicsCollider>>()
                         .WithAll<DeathTag>()
                         .WithDisabled<ResurrectTag>().WithNone<DeathInitialized>().WithEntityAccess())
            {
                Log.Debug($"Killing {entity}");

                foreach (var child in state.EntityManager.GetBuffer<Child>(entity))
                {
                    if (!state.EntityManager.HasComponent<MaterialPropertyBaseColor>(child.Value))
                        continue;

                    var a = state.EntityManager.GetComponentData<MaterialPropertyBaseColor>(child.Value);
                    var b = state.EntityManager.GetComponentData<ColorDeath>(child.Value);
                    a.Value = b.Value.ToFloat4();
                    state.EntityManager.SetComponentData(child.Value, a);
                }
                speed.ValueRW.Value *= 2.0f;
                ecb.SetComponentEnabled<MoveTowardsTargetTag>(entity, false);
                ecb.SetComponentEnabled<OverlapBox>(entity, false);
                ecb.SetComponentEnabled<MoveFreeTag>(entity, true);
                ecb.SetComponentEnabled<MoveClassicTag>(entity, false);
                
                ecb.AddComponent<DeathInitialized>(entity);
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
    
}