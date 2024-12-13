using System.Globalization;
using Unity.Entities;
using Natrium.Gameplay.Shared.Components;
using Natrium.Gameplay.Shared.Components.Input;
using Natrium.Shared;
using Natrium.Shared.Extensions;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Burst;
using Unity.NetCode;

namespace Natrium.Gameplay.Server.Systems
{
    [UpdateInGroup(typeof(GhostSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct AimSystem : ISystem, ISystemStartStop
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
            
            foreach (var (inputAim, physicsCollider, ghostOwner, entity) 
                     in SystemAPI.Query<RefRO<InputAim>, RefRO<PhysicsCollider>, RefRO<GhostOwner>>()
                        .WithEntityAccess())
            {
                if (!inputAim.ValueRO.InputEvent.IsSet)
                    continue;

                var interpolationDelayTicks = networkTime.ServerTick;
                interpolationDelayTicks.Subtract(inputAim.ValueRO.ServerTick.TickIndexForValidTick);
                
                SystemAPI.GetSingleton<PhysicsWorldHistorySingleton>().GetCollisionWorldFromTick(
                        inputAim.ValueRO.ServerTick, 
                        interpolationDelayTicks.TickIndexForValidTick,
                        ref physicsWorld.ValueRW.PhysicsWorld, 
                        out var collisionHistoryWorld );

                Log.Debug($"{nameof(InputAim)} from {entity}@{inputAim.ValueRO.ServerTick}: {inputAim.ValueRO.MouseWorldPosition.ToString("F2", CultureInfo.InvariantCulture)}");

                var offset = new float3(0, 10, 0); //ToDo: The plus 10 on y axis, comes from the offset of the camara
                var raycastInput = new RaycastInput
                {
                    Start = inputAim.ValueRO.MouseWorldPosition + offset,
                    End = inputAim.ValueRO.MouseWorldPosition,
                    Filter = physicsCollider.ValueRO.Value.Value.GetCollisionFilter()
                };

                if (collisionHistoryWorld.CastRay(raycastInput, out var closestHit))
                {
                    Log.Debug($"{nameof(InputAim)} from: {entity} -> Collides with: {closestHit.Entity}");
                    
                    if (closestHit.Entity == Entity.Null)
                    {
                        Log.Error($"Collision with {closestHit.Entity} is Null");
                        continue;
                    }
                    
                    if (state.EntityManager.HasComponent<AttacksBuffer>(closestHit.Entity) &&
                        !state.EntityManager.IsComponentEnabled<DeathTag>(closestHit.Entity))
                    {
                        var attacksBuffer = state.EntityManager.GetBuffer<AttacksBuffer>(closestHit.Entity);
                        
                        Log.Debug($"Attack In Progress@|{inputAim.ValueRO.ServerTick}");

                        attacksBuffer.Add(new AttacksBuffer
                        {
                            EntitySource = entity,
                            EntityTarget = closestHit.Entity,
                        });
                    }
                }
            }
        }
    } //AimSystem
}