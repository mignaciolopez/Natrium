using Natrium.Gameplay.Client.Components.UI.Debug;
using Natrium.Gameplay.Shared.Components;
using Natrium.Gameplay.Shared.Components.Input;
using Natrium.Shared;
//using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using Unity.Mathematics;
using Unity.Transforms;
using Natrium.Shared.Extensions;
using Unity.Physics;

namespace Natrium.Gameplay.Client.Systems.UI.Debug
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct DebugAimSystem : ISystem, ISystemStartStop
    {
        //[BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            Log.Verbose("OnCreate");
            state.RequireForUpdate<DebugAimInputPrefab>();
            state.RequireForUpdate<NetworkTime>();
            state.RequireForUpdate<InputAim>();
            state.RequireForUpdate<PhysicsWorldSingleton>();
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

            if (!networkTime.IsFirstTimeFullyPredictingTick)
                return;
            
            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
            
            foreach (var (inputAim, physicsCollider, entity)
                     in SystemAPI.Query<RefRO<InputAim>, RefRO<PhysicsCollider>>()
                         .WithAll<Simulate>()
                         .WithEntityAccess())
            {
                if (!inputAim.ValueRO.InputEvent.IsSet)
                    continue;

                Log.Debug($"Processing {entity}'s {nameof(InputAim)}@{inputAim.ValueRO.ServerTick}|{networkTime.ServerTick}");

                var raycastInput = new RaycastInput
                {
                    Start = inputAim.ValueRO.Origin,
                    End = inputAim.ValueRO.Origin + inputAim.ValueRO.Direction * 30,
                    Filter = physicsCollider.ValueRO.Value.Value.GetCollisionFilter(),
                };

                if (collisionWorld.CastRay(raycastInput, out var closestHit))
                {
                    var prefabEntity = SystemAPI.GetSingleton<DebugAimInputPrefab>().Prefab;
                    var prefabLocalTransform = state.EntityManager.GetComponentData<LocalTransform>(prefabEntity);
                
                    var debugEntity = state.EntityManager.Instantiate(prefabEntity);

                    var position = math.round(closestHit.Position);
                    position.y = 0.01f;
                    
                    state.EntityManager.SetComponentData(debugEntity, new LocalTransform
                    {
                        Position = position,
                        Rotation = prefabLocalTransform.Rotation,
                        Scale = prefabLocalTransform.Scale,
                    });

                    var color = UnityEngine.Color.white;
                
                    foreach (var child in state.EntityManager.GetBuffer<Child>(entity))
                    {
                        if (state.EntityManager.HasComponent<MaterialPropertyBaseColor>(child.Value))
                        {
                            color = state.EntityManager.GetComponentData<MaterialPropertyBaseColor>(child.Value).Value.ToColor();
                            break;
                        }
                    }
                
                    color.a = 0.5f;
                
                    var spriteRenderer = state.EntityManager.GetComponentObject<UnityEngine.SpriteRenderer>(debugEntity);
                    spriteRenderer.color = color;
                }
            }
        }
    }
}
