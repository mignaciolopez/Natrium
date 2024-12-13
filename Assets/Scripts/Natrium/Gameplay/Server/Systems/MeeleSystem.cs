using System.Globalization;
using Natrium.Gameplay.Shared.Components;
using Natrium.Gameplay.Shared.Components.Input;
using Natrium.Shared;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;

namespace Natrium.Gameplay.Server.Systems
{
    [UpdateInGroup(typeof(GhostSimulationSystemGroup))]
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
            
            foreach (var (inputMelee, physicsCollider, ghostOwner, entity) 
                     in SystemAPI.Query<RefRO<InputMelee>, RefRO<PhysicsCollider>, RefRO<GhostOwner>>()
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
                    //Start = player position 
                    //End = player forward vector + 1
                    Filter = physicsCollider.ValueRO.Value.Value.GetCollisionFilter()
                };

                if (collisionHistoryWorld.CastRay(raycastInput, out var closestHit))
                {
                    Log.Debug($"{nameof(InputMelee)} from: {entity} -> Collides with: {closestHit.Entity}");
                    
                    if (closestHit.Entity == Entity.Null)
                    {
                        Log.Error($"Collision with {closestHit.Entity} is Null");
                        continue;
                    }
                    
                    if (state.EntityManager.HasComponent<AttacksBuffer>(closestHit.Entity) &&
                        !state.EntityManager.IsComponentEnabled<DeathTag>(closestHit.Entity))
                    {
                        var attacksBuffer = state.EntityManager.GetBuffer<AttacksBuffer>(closestHit.Entity);
                        
                        Log.Debug($"Attack In Progress@|{inputMelee.ValueRO.ServerTick}");

                        attacksBuffer.Add(new AttacksBuffer
                        {
                            EntitySource = entity,
                            EntityTarget = closestHit.Entity,
                        });
                    }
                }
            }
        }
    }
}
