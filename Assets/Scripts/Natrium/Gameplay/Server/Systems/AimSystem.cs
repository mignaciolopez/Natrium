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
            Log.Verbose($"[{state.WorldUnmanaged.Name}] | {this.ToString()}.OnCreate()");
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate<PhysicsWorldHistorySingleton>();
            state.RequireForUpdate<NetworkTime>();
        }

        //[BurstCompile]
        public void OnStartRunning(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] | {this.ToString()}.OnStartRunning()");
            //_bsEcbS = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
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
            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
            
            var currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
            if (!currentTick.IsValid)
            {
                Log.Warning($"currentTick is Invalid!");
                return;
            }
            
            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
            foreach (var (inputAims, pc, e) 
                     in SystemAPI.Query<DynamicBuffer<InputAim>, RefRO<PhysicsCollider>>()
                         .WithAll<Simulate, DamageDealerTag>().WithEntityAccess())
            {
                inputAims.GetDataAtTick(currentTick, out var inputAimAtTick);
                if (!inputAimAtTick.Set)
                {
                    //Log.Debug($"inputAimAtTick {currentTick} is not Set.");
                    continue;
                }

                Log.Debug($"AimInput from {e}: {inputAimAtTick.MouseWorldPosition.ToString("F2", CultureInfo.InvariantCulture)}");

                var offset = new float3(0, 10, 0); //ToDo: The plus 10 on y axis, comes from the offset of the camara
                var raycastInput = new RaycastInput
                {
                    Start = inputAimAtTick.MouseWorldPosition + offset,
                    End = inputAimAtTick.MouseWorldPosition,
                    Filter = pc.ValueRO.Value.Value.GetCollisionFilter()
                };

                if (collisionWorld.CastRay(raycastInput, out var closestHit))
                {
                    Log.Debug($"AimInput from: {e} -> Collides with: {closestHit.Entity}");
                    
                    if (closestHit.Entity == Entity.Null)
                    {
                        Log.Warning($"Collision with {closestHit.Entity} is Null");
                        continue;
                    }

                    if (state.EntityManager.HasComponent<Attack>(closestHit.Entity))
                    {
                        ecb.SetComponentEnabled<Attack>(closestHit.Entity, true);
                        ecb.SetComponent(closestHit.Entity, new Attack
                        {
                            SourceServerEntity = e
                        });
                    }
                }
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    } //AimSystem
}