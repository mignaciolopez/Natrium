using Natrium.Gameplay.Shared.Components;
using Natrium.Shared;
using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;
using Unity.Physics;

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
                         physicsCollider, hp,dt, rt, e)
                     in SystemAPI.Query<RefRW<MovementType>, RefRW<Speed>, RefRW<DebugColor>, RefRW<PhysicsCollider>,
                        RefRW<HealthPoints>, EnabledRefRW<DeathTag>, EnabledRefRW<ResurrectTag>>()
                         .WithEntityAccess())
            {
                Log.Debug($"Resurrecting {e}");

                hp.ValueRW.Value = hp.ValueRO.MaxValue;
                
                movementType.ValueRW.Value = MovementTypeEnum.Classic;
                speed.ValueRW.Value = 4.0f;
                debugColor.ValueRW.Value = debugColor.ValueRO.StartValue;
                
                ecb.SetComponentEnabled<Attack>(e, true);
                var collisionFilter = physicsCollider.ValueRO.Value.Value.GetCollisionFilter();
                collisionFilter.CollidesWith = ~0u;
                physicsCollider.ValueRW.Value.Value.SetCollisionFilter(collisionFilter);
                
                Log.Debug($"Setting DeathTag to false on {e}");
                dt.ValueRW = false;
                Log.Debug($"Setting ResurrectTag to false on {e}");
                rt.ValueRW = false;
                
                ecb.RemoveComponent<DeathInitialized>(e);
                //ecb.SetComponentEnabled<DeathTag>(e, false);
                //ecb.SetComponentEnabled<ResurrectTag>(e, false);
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
    
}