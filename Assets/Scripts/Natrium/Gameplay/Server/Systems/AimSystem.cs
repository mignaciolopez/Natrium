using Unity.Entities;
using Natrium.Gameplay.Shared.Components;
using Natrium.Shared;
using Unity.Physics;
using Unity.Burst;
using Natrium.Gameplay.Server.Components;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.NetCode;

namespace Natrium.Gameplay.Server.Systems
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct AimSystem : ISystem, ISystemStartStop
    {
        private EntityCommandBuffer _ecb;

        //[BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] | {this.ToString()}.OnCreate()");
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate<PhysicsWorldHistorySingleton>();
            state.RequireForUpdate<NetworkTime>();
        }

        public void OnStartRunning(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] | {this.ToString()}.OnStartRunning()");
        }

        public void OnStopRunning(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] | {this.ToString()}.OnStopRunning()");
        }

        public void OnDestroy(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] | {this.ToString()}.OnDestroy()");
        }
        
        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbs = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            _ecb = ecbs.CreateCommandBuffer(state.WorldUnmanaged);

            var networkTime = SystemAPI.GetSingleton<NetworkTime>();

            foreach (var (ai, pc, dp, e) in SystemAPI.Query<RefRW<AimInput>, PhysicsCollider, DamagePoints>().WithAll<Simulate, DamageDealerTag>().WithEntityAccess())
            {
                if (ai.ValueRO.AimInputEvent.IsSet)
                {
                    var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;

                    Log.Debug($"AimInput from {e}: {ai.ValueRO.Value.ToString("0.00", null)}");

                    var start = ai.ValueRO.Value;
                    start.y = 10.0f; //ToDo: The plus 10 on y axis, comes from the offset of the camara
                    var raycastInput = new RaycastInput
                    {
                        Start = start,
                        End = ai.ValueRO.Value,
                        Filter = pc.Value.Value.GetCollisionFilter()
                    };

                    if (collisionWorld.CastRay(raycastInput, out var closestHit))
                    {
                        if (closestHit.Entity == Entity.Null)// || closestHit.Entity == e)
                        {
                            Log.Warning($"Hited Entity {closestHit.Entity} is Null or Self");
                            continue;
                        }

                        if (state.EntityManager.HasComponent<AttackableTag>(closestHit.Entity))
                        {
                            Log.Debug($"Entity {e} is Dealing Damage to {closestHit.Entity}");
                            var damageBuffer = state.EntityManager.GetBuffer<DamagePointsBuffer>(closestHit.Entity);
                            damageBuffer.Add(new DamagePointsBuffer { Value = dp.Value });

                            //SpawnDebugData(ref state, e, closestHit);
                        }
                    }
                }
            }
        }

        private void SpawnDebugData(ref SystemState state, Entity e, RaycastHit closestHit)
        {
            var debugTile = SystemAPI.GetSingleton<DebugAttackPrefab>().Value;

            var tile = state.EntityManager.Instantiate(debugTile);

            var roundedPosition = math.round(closestHit.Position);
            roundedPosition.y = closestHit.Position.y + 0.1f;

            state.EntityManager.SetComponentData(tile, new LocalTransform
            {
                Position = roundedPosition,
                Rotation = quaternion.identity,
                Scale = 1.0f
            });

            var color = state.EntityManager.GetComponentData<DebugColor>(e).Value;

            state.EntityManager.SetComponentData(tile, new DebugColor
            {
                Value = color
            });

            var childs = SystemAPI.GetBuffer<LinkedEntityGroup>(tile);
            foreach (var child in childs)
            {
                if (state.EntityManager.HasComponent<UnityEngine.SpriteRenderer>(child.Value))
                {
                    state.EntityManager.GetComponentObject<UnityEngine.SpriteRenderer>(child.Value).color = new UnityEngine.Color(color.x, color.y, color.z);
                }
            }
        }
    } //AimSystem
}