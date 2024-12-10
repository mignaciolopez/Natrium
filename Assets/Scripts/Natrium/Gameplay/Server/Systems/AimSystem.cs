using System.Globalization;
using Unity.Entities;
using Natrium.Gameplay.Shared.Components;
using Natrium.Gameplay.Shared.Components.Input;
using Natrium.Shared;
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
            
            foreach (var (inputAims, pc, ghostOwner, entity) 
                     in SystemAPI.Query<DynamicBuffer<InputAim>, RefRO<PhysicsCollider>, RefRO<GhostOwner>>()
                         .WithAll<Simulate, DamageDealerTag>().WithEntityAccess())
            {
                if (!inputAims.GetDataAtTick(networkTime.ServerTick, out var inputAimAtTick))
                {
                    Log.Warning($"No {nameof(InputAim)}@{networkTime.ServerTick}");
                    continue;
                }
                
                if (!inputAimAtTick.Set)
                    continue;
                
                var interpolationDelay = networkTime.ServerTick;
                interpolationDelay.Subtract(inputAimAtTick.Tick.TickIndexForValidTick);
                
                SystemAPI.GetSingleton<PhysicsWorldHistorySingleton>().GetCollisionWorldFromTick(
                        inputAimAtTick.Tick, 
                        interpolationDelay.TickIndexForValidTick,
                        ref physicsWorld.ValueRW.PhysicsWorld, 
                        out var collisionHistoryWorld );

                Log.Debug($"{nameof(InputAim)} from {entity}@{inputAimAtTick.Tick}: {inputAimAtTick.MouseWorldPosition.ToString("F2", CultureInfo.InvariantCulture)}");

                var offset = new float3(0, 10, 0); //ToDo: The plus 10 on y axis, comes from the offset of the camara
                var raycastInput = new RaycastInput
                {
                    Start = inputAimAtTick.MouseWorldPosition + offset,
                    End = inputAimAtTick.MouseWorldPosition,
                    Filter = pc.ValueRO.Value.Value.GetCollisionFilter()
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
                        
                        Log.Debug($"Attack In Progress@{inputAimAtTick.Tick}|{networkTime.ServerTick}");

                        attacksBuffer.Add(new AttacksBuffer
                        {
                            ServerTick = networkTime.ServerTick,
                            InterpolationTick = networkTime.InterpolationTick,
                            EntitySource = entity,
                            NetworkIdSource = ghostOwner.ValueRO.NetworkId,
                        });
                    }
                }
            }
        }
    } //AimSystem
}