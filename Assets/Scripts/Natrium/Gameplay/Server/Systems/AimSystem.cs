using System.Globalization;
using Unity.Entities;
using Natrium.Gameplay.Shared.Components;
using Natrium.Gameplay.Shared.Components.Input;
using Natrium.Shared;
using Unity.Mathematics;
using Unity.Physics;
//using Unity.Burst;
using Unity.NetCode;

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
            //var ecb = _bsEcbS.CreateCommandBuffer(state.WorldUnmanaged);
            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
            
            foreach (var (ai, pc, dp, e) in SystemAPI.Query<RefRW<InputAim>, RefRO<PhysicsCollider>, RefRO<DamagePoints>>().WithAll<Simulate, DamageDealerTag>().WithEntityAccess())
            {
                if (!ai.ValueRO.InputEvent.IsSet)
                    continue;

                Log.Debug($"AimInput from {e}: {ai.ValueRO.MouseWorldPosition.ToString("F2", CultureInfo.InvariantCulture)}");

                var offset = new float3(0, 10, 0); //ToDo: The plus 10 on y axis, comes from the offset of the camara
                var raycastInput = new RaycastInput
                {
                    Start = ai.ValueRO.MouseWorldPosition + offset,
                    End = ai.ValueRO.MouseWorldPosition,
                    Filter = pc.ValueRO.Value.Value.GetCollisionFilter()
                };

                if (collisionWorld.CastRay(raycastInput, out var closestHit))
                {
                    Log.Debug($"AimInput from: {e} -> Collides with: {closestHit.Entity}");
                    
                    if (closestHit.Entity == Entity.Null)// || closestHit.Entity == e)
                    {
                        Log.Warning($"Collision with {closestHit.Entity} is Null or Self");
                        continue;
                    }

                    if (state.EntityManager.HasComponent<AttackableTag>(closestHit.Entity))
                    {
                        Log.Debug($"{e} is Dealing Damage on {closestHit.Entity}");
                        var damageBuffer = state.EntityManager.GetBuffer<DamagePointsBuffer>(closestHit.Entity);
                        damageBuffer.Add(new DamagePointsBuffer { Value = dp.ValueRO.Value });
                    }
                }
            }
        }
    } //AimSystem
}