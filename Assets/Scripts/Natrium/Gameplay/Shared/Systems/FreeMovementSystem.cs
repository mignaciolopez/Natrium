using Natrium.Gameplay.Shared.Components;
using Natrium.Gameplay.Shared.Components.Input;
using Natrium.Shared;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;

namespace Natrium.Gameplay.Shared.Systems
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [UpdateBefore(typeof(MovementSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial struct FreeMovementSystem : ISystem, ISystemStartStop
    {
        //[BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] OnCreate");
            state.RequireForUpdate<PhysicsWorldSingleton>();
        }
        
        public void OnStartRunning(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] OnStartRunning");
        }
        
        public void OnStopRunning(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] OnStopRunning");
        }
        
        public void OnDestroy(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] OnDestroy");
        }

        [BurstCompile]
        public unsafe void OnUpdate(ref SystemState state)
        {
            var dt = SystemAPI.Time.DeltaTime;

            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;

            foreach (var (ptp, lt, pia, speed, pc, e) 
                     in SystemAPI.Query<RefRW<PlayerTilePosition>, LocalTransform, InputMove, Speed, PhysicsCollider>()
                         .WithAll<Simulate, GhostOwner, MovementFree>().WithEntityAccess())
            {
                ptp.ValueRW.Previous = ptp.ValueRO.Target;

                var input = new float3(pia.InputAxis.x, 0.0f, pia.InputAxis.y);

                ptp.ValueRW.Target = lt.Position + speed.Value * dt * input;

                if (ptp.ValueRW.Previous.x != ptp.ValueRO.Target.x ||
                    ptp.ValueRW.Previous.z != ptp.ValueRO.Target.z)
                {

                    var colliderCastInput = new ColliderCastInput
                    {
                        Start = ptp.ValueRO.Previous,
                        End = ptp.ValueRO.Target,
                        Collider = pc.ColliderPtr,
                        Orientation = lt.Rotation,
                        QueryColliderScale = 0.9f
                    };

                    var allHits = new NativeList<ColliderCastHit>(Allocator.Temp);

                    if (collisionWorld.CastCollider(colliderCastInput, ref allHits))
                    {
                        foreach (var hit in allHits)
                        {
                            if (hit.Entity == e || hit.Entity == Entity.Null)
                            {
                                //Log.Verbose($"Ignoring CastCollider {e} hit with {hit.Entity}");
                                continue;
                            }
                            else
                            {
                                //Log.Info($"CastCollider {e} hit with {hit.Entity}");
                                ptp.ValueRW.Target = ptp.ValueRO.Previous;
                                break;
                            }
                        }
                    }
                    allHits.Dispose();
                }
            }
        }
    } //MovementSystem
} // namespace