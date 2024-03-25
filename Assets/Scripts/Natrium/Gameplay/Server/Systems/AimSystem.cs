using Unity.Entities;
using Natrium.Gameplay.Shared.Components;
using Natrium.Shared;
using Unity.Physics;
using Unity.Burst;
using Unity.Physics.Systems;
using Natrium.Gameplay.Server.Components;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.NetCode;

namespace Natrium.Gameplay.Server.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(PhysicsSystemGroup))]
    [UpdateAfter(typeof(PhysicsSimulationGroup))]
    public partial struct AimSystem : ISystem
    {
        private EntityCommandBuffer _ecb;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();
            //state.RequireForUpdate<PhysicsWorldHistorySingleton>();
            state.RequireForUpdate<NetworkTime>();
        }

        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();
            if (!networkTime.IsFirstTimeFullyPredictingTick)
                return;

            foreach (var (ai, pc, dp, e) in SystemAPI.Query<AimInput, PhysicsCollider, DamagePoints>().WithAll<DamageDealerTag>().WithEntityAccess())
            {
                if (ai.Input.IsSet)
                {
                    var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;

                    Log.Debug($"AimInput from {e}: {ai.Value.ToString("0.00", null)}");

                    var start = ai.Value;
                    start.y = 10.0f; //ToDo: The plus 10 on y axis, comes from the offset of the camara
                    var raycastInput = new RaycastInput
                    {
                        Start = start,
                        End = ai.Value,
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

                            SpawnDebugData(ref state, e, closestHit);
                        }
                    }
                }
            }

            _ecb.Playback(state.EntityManager);
            _ecb.Dispose();
        }

        private void SpawnDebugData(ref SystemState state, Entity e, RaycastHit closestHit)
        {
            var debugTile = SystemAPI.GetSingleton<DebugAttackPrefab>().Value;

            var tile = _ecb.Instantiate(debugTile);

            var roundedPosition = math.round(closestHit.Position);
            roundedPosition.y = closestHit.Position.y + 0.1f;

            _ecb.SetComponent(tile, new LocalTransform
            {
                Position = roundedPosition,
                Rotation = quaternion.identity,
                Scale = 1.0f
            });

            _ecb.SetComponent(tile, new DestroyOnTimer { Value = 1.0f });

            var DebugColor = state.EntityManager.GetComponentData<DebugColor>(e);
            var leg = state.GetBufferLookup<LinkedEntityGroup>(true)[e];
            foreach (var child in leg)
            {
                if (state.EntityManager.HasComponent<UnityEngine.SpriteRenderer>(child.Value))
                {
                    var sr = state.EntityManager.GetComponentObject<UnityEngine.SpriteRenderer>(child.Value);
                    sr.color = new UnityEngine.Color(DebugColor.Value.x, DebugColor.Value.y, DebugColor.Value.z);
                    break;
                }
            }
        }
    } //AimSystem
}