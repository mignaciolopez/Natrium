using Natrium.Gameplay.Shared.Components;
using Natrium.Gameplay.Shared.Components.Input;
using Natrium.Shared;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;

namespace Natrium.Gameplay.Server.Systems
{
    [UpdateInGroup(typeof(AttackSystemGroup))]
    [UpdateBefore(typeof(AttackSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct MeeleSystem : ISystem, ISystemStartStop
    {
        //[BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            Log.Verbose("OnCreate");
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate<PhysicsWorldHistorySingleton>();
            state.RequireForUpdate<NetworkTime>();
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
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();
            var physicsWorld = SystemAPI.GetSingletonRW<PhysicsWorldSingleton>();
            
            foreach (var (inputMelee, physicsCollider, localTransform, entity) 
                     in SystemAPI.Query<RefRO<InputMelee>, RefRO<PhysicsCollider>, RefRO<LocalTransform>>()
                         .WithDisabled<DeathTag>()
                         .WithEntityAccess())
            {
                if (!inputMelee.ValueRO.InputEvent.IsSet)
                    continue;
                
                var interpolationDelayTicks = networkTime.ServerTick;
                interpolationDelayTicks.Subtract(inputMelee.ValueRO.ServerTick.TickIndexForValidTick);
                
                SystemAPI.GetSingleton<PhysicsWorldHistorySingleton>().GetCollisionWorldFromTick(
                        inputMelee.ValueRO.ServerTick, 
                        interpolationDelayTicks.TickIndexForValidTick,
                        ref physicsWorld.ValueRW.PhysicsWorld, 
                        out var collisionHistoryWorld );

                Log.Debug($"{nameof(InputMelee)} from {entity}@{inputMelee.ValueRO.ServerTick}");
                
                var raycastInput = new RaycastInput
                {
                    Start = localTransform.ValueRO.Position,
                    End = localTransform.ValueRO.Position + math.forward(localTransform.ValueRO.Rotation),
                    Filter = physicsCollider.ValueRO.Value.Value.GetCollisionFilter()
                };

                var outHits = new NativeList<RaycastHit>(state.WorldUpdateAllocator);
                
                if (collisionHistoryWorld.CastRay(raycastInput, ref outHits))
                {
                    foreach (var hit in outHits)
                    {
                        if (hit.Entity != entity)
                        {
                            if (state.EntityManager.HasComponent<DeathTag>(hit.Entity) &&
                                state.EntityManager.IsComponentEnabled<DeathTag>(hit.Entity))
                                continue;
                            
                            Log.Debug($"{nameof(InputMelee)} from: {entity} -> Collides with: {hit.Entity}");
                
                            if (hit.Entity == Entity.Null)
                            {
                                Log.Error($"Collision with {hit.Entity} is Null");
                                continue;
                            }

                            if (state.EntityManager.HasComponent<AttacksBuffer>(hit.Entity) &&
                                !state.EntityManager.IsComponentEnabled<DeathTag>(hit.Entity))
                            {
                                var attacksBuffer = state.EntityManager.GetBuffer<AttacksBuffer>(hit.Entity);

                                Log.Debug($"Attack In Progress@{inputMelee.ValueRO.ServerTick}|{networkTime.ServerTick}");

                                attacksBuffer.Add(new AttacksBuffer
                                {
                                    EntitySource = entity,
                                    EntityTarget = hit.Entity,
                                });
                            }
                        }
                    }
                }

                outHits.Dispose();
            }
        }
    }
}
