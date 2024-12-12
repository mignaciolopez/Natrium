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
            QueryAndShowInputAims(ref state);
        }

        //[BurstCompile]
        private void QueryAndShowInputAims(ref SystemState state)
        {
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();

            if (!networkTime.IsFirstTimeFullyPredictingTick)
                return;
            
            foreach (var (inputAim, entity)
                     in SystemAPI.Query<RefRO<InputAim>>()
                         .WithAll<Simulate>()
                         .WithEntityAccess())
            {
                if (!inputAim.ValueRO.InputEvent.IsSet)
                    continue;
                
                Log.Debug($"Processing {entity} {nameof(InputAim)}@{inputAim.ValueRO.ServerTick}|{networkTime.ServerTick}");
                
                var prefabEntity = SystemAPI.GetSingleton<DebugAimInputPrefab>().Prefab;
                var prefabLocalTransform = state.EntityManager.GetComponentData<LocalTransform>(prefabEntity);
                
                var debugEntity = state.EntityManager.Instantiate(prefabEntity);
                
                state.EntityManager.SetComponentData(debugEntity, new LocalTransform
                {
                    Position = math.round(new float3(inputAim.ValueRO.MouseWorldPosition.x, 5.0f, inputAim.ValueRO.MouseWorldPosition.z)),
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
