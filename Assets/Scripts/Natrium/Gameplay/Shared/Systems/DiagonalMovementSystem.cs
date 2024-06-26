using Natrium.Gameplay.Shared.Components;
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
    public partial struct DiagonalMovementSystem : ISystem, ISystemStartStop
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();
        }

        [BurstCompile]
        public void OnStartRunning(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnStopRunning(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var dt = SystemAPI.Time.DeltaTime;

            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;

            foreach (var (ptp, lt, pia, speed, pc, e) in SystemAPI.Query<RefRW<PlayerTilePosition>, LocalTransform, PlayerInputAxis, Speed, PhysicsCollider>().WithAll<Simulate, GhostOwner, MovementDiagonal>().WithEntityAccess())
            {
                if (math.distance(lt.Position, ptp.ValueRO.Target) < speed.Value * dt)
                {
                    ptp.ValueRW.Target = math.round(lt.Position);
                    ptp.ValueRW.Previous = ptp.ValueRO.Target;

                    var input = new float3(pia.Value.x, 0.0f, pia.Value.y);

                    switch (input.x)
                    {
                        case > 0:
                            ptp.ValueRW.Target.x++;
                            break;
                        case < 0:
                            ptp.ValueRW.Target.x--;
                            break;
                    }

                    switch (input.z)
                    {
                        case > 0:
                            ptp.ValueRW.Target.z++;
                            break;
                        case < 0:
                            ptp.ValueRW.Target.z--;
                            break;
                    }

                    if (ptp.ValueRW.Previous.x != ptp.ValueRO.Target.x ||
                        ptp.ValueRW.Previous.z != ptp.ValueRO.Target.z)
                    {

                        var raycastInput = new RaycastInput
                        {
                            Start = lt.Position,
                            End = lt.Position + input * 2.0f,
                            Filter = pc.Value.Value.GetCollisionFilter()
                        };

                        var allHits = new NativeList<RaycastHit>(Allocator.Temp);

                        if (collisionWorld.CastRay(raycastInput, ref allHits))
                        {
                            foreach (var hit in allHits)
                            {
                                if (hit.Entity == e || hit.Entity == Entity.Null)
                                {
                                    //Log.Verbose($"Ignoring CastRay {e} hit with {hit.Entity}");
                                    continue;
                                }
                                else
                                {
                                    if (math.distance(hit.Position, lt.Position) <= 1.0f)
                                    {
                                        //Log.Info($"CastRay {e} hit with {hit.Entity}");
                                        ptp.ValueRW.Target = ptp.ValueRO.Previous;
                                        break;
                                    }
                                }
                            }
                        }

                        allHits.Dispose();
                    }
                }
            }
        }
    }
} // namespace