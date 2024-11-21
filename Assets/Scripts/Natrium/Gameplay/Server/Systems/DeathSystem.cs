using Natrium.Gameplay.Shared.Components;
using Natrium.Shared;
using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;
using Unity.Physics;

namespace Natrium.Gameplay.Server.Systems
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderLast = true)]
    [UpdateAfter(typeof(HealthSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct DeathSystem : ISystem, ISystemStartStop
    {
        private EndSimulationEntityCommandBufferSystem.Singleton _esEcbS;
        
        //[BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] | {this.ToString()}.OnCreate()");
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<NetworkTime>();
        }

        //[BurstCompile]
        public void OnStartRunning(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] | {this.ToString()}.OnStartRunning()");
            _esEcbS = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        //[BurstCompile]
        public void OnStopRunning(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] | {this.ToString()}.OnStopRunning()");
        }

        //[BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] | {this.ToString()}.OnDestroy()");
        }

        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);

            foreach (var (movementType, speed, debugColor,
                         physicsCollider, dt, e)
                     in SystemAPI.Query<RefRW<MovementType>, RefRW<Speed>, RefRW<DebugColor>,
                             RefRW<PhysicsCollider>, RefRO<DeathTag>>()
                         .WithDisabled<ResurrectTag>().WithNone<DeathInitialized>().WithEntityAccess())
            {
                Log.Debug($"Killing {e}");
                movementType.ValueRW.Value = MovementTypeEnum.Free;
                speed.ValueRW.Value = 8.0f;
                debugColor.ValueRW.Value = debugColor.ValueRO.DeathValue;
                
                ecb.SetComponent(e, new Attack());
                ecb.SetComponentEnabled<Attack>(e, false);
                
                var collisionFilter = physicsCollider.ValueRO.Value.Value.GetCollisionFilter();
                collisionFilter.CollidesWith = 0u;
                physicsCollider.ValueRW.Value.Value.SetCollisionFilter(collisionFilter);
                ecb.AddComponent<DeathInitialized>(e);
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
    
}