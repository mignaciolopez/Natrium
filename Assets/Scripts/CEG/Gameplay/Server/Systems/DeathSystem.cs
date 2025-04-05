using CEG.Gameplay.Shared.Components;
using CEG.Shared;
using CEG.Shared.Extensions;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;

namespace CEG.Gameplay.Server.Systems
{
    [UpdateInGroup(typeof(AttackSystemGroup))]
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
                speed.ValueRW.Translation *= 2.0f;
                
                ecb.AddComponent<DeathInitialized>(entity);
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
    
}