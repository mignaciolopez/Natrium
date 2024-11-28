using System.Globalization;
using Unity.Entities;
using Natrium.Gameplay.Shared.Components;
using Natrium.Gameplay.Shared.Components.Input;
using Natrium.Shared;
using Unity.Mathematics;
using Unity.Physics;
//using Unity.Burst;
using Unity.NetCode;
using UnityEngine;

namespace Natrium.Gameplay.Server.Systems
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct AimSystem : ISystem, ISystemStartStop
    {
        //private BeginSimulationEntityCommandBufferSystem.Singleton _bsEcbS;

        //[BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            Log.Verbose("OnCreate");
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate<PhysicsWorldHistorySingleton>();
            state.RequireForUpdate<NetworkTime>();
        }

        //[BurstCompile]
        public void OnStartRunning(ref SystemState state)
        {
            Log.Verbose("OnStartRunning");
            //_bsEcbS = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
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
            
            var currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
            if (!currentTick.IsValid)
            {
                Log.Warning($"currentTick is Invalid!");
                return;
            }
            
            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
            foreach (var (inputAims, pc, ghostOwner, entity) 
                     in SystemAPI.Query<DynamicBuffer<InputAim>, RefRO<PhysicsCollider>, RefRO<GhostOwner>>()
                         .WithAll<Simulate, DamageDealerTag>().WithEntityAccess())
            {
                inputAims.GetDataAtTick(currentTick, out var inputAimAtTick);
                if (!inputAimAtTick.Set)
                {
                    //Log.Debug($"inputAimAtTick {currentTick} is not Set.");
                    continue;
                }

                Log.Debug($"AimInput from {entity}: {inputAimAtTick.MouseWorldPosition.ToString("F2", CultureInfo.InvariantCulture)}");

                var offset = new float3(0, 10, 0); //ToDo: The plus 10 on y axis, comes from the offset of the camara
                var raycastInput = new RaycastInput
                {
                    Start = inputAimAtTick.MouseWorldPosition + offset,
                    End = inputAimAtTick.MouseWorldPosition,
                    Filter = pc.ValueRO.Value.Value.GetCollisionFilter()
                };

                if (collisionWorld.CastRay(raycastInput, out var closestHit))
                {
                    Log.Debug($"AimInput from: {entity} -> Collides with: {closestHit.Entity}");
                    
                    if (closestHit.Entity == Entity.Null)
                    {
                        Log.Warning($"Collision with {closestHit.Entity} is Null");
                        continue;
                    }
                    
                    if (state.EntityManager.HasComponent<GhostOwner>(closestHit.Entity) &&
                        !state.EntityManager.IsComponentEnabled<DeathTag>(closestHit.Entity))
                    {
                        var networkIdTarget = state.EntityManager.GetComponentData<GhostOwner>(closestHit.Entity);
                    
                        Log.Debug($"Attack Event In Progress on Tick {currentTick}");
                        
                        foreach (var attackEvents in SystemAPI.Query<DynamicBuffer<AttackEvents>>().WithAll<Simulate>())
                        {
                            var attackEvent = new AttackEvents
                            {
                                NetworkTick = currentTick,
                                EntitySource = entity,
                                EntityTarget = closestHit.Entity,
                                NetworkIdSource = ghostOwner.ValueRO.NetworkId,
                                NetworkIdTarget = networkIdTarget.NetworkId,
                            };
                            attackEvents.Add(attackEvent);
                        }
                    }
                }
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    } //AimSystem
}