using Natrium.Gameplay.Client.Components.UI.Debug;
using Natrium.Gameplay.Shared.Components;
using Natrium.Gameplay.Shared.Components.Input;
using Natrium.Shared;
//using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using Unity.Mathematics;
using Unity.Transforms;

namespace Natrium.Gameplay.Client.Systems.UI.Debug
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderLast = true)]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct DebugAimSystem : ISystem, ISystemStartStop
    {
        //[BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] | {this.ToString()}.OnCreate()");
            state.RequireForUpdate<DebugAimInputPrefab>();
            state.RequireForUpdate<NetworkTime>();
            state.RequireForUpdate<InputAim>();
            state.RequireForUpdate<DebugColor>();
            state.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
        }

        //[BurstCompile]
        public void OnStartRunning(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] | {this.ToString()}.OnStartRunning()");
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
            QueryAndShowInputAims(ref state);
        }

        //[BurstCompile]
        private void QueryAndShowInputAims(ref SystemState state)
        {
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();
            if (!networkTime.IsFirstTimeFullyPredictingTick)
            {
                //Log.Debug($"IsFirstTimeFullyPredictingTick is false");
                return;
            }
            
            var currentTick = networkTime.ServerTick;
            if (!currentTick.IsValid)
            {
                Log.Warning($"currentTick is Invalid!");
                return;
            }
            
            foreach (var (inputAims, debugColor)
                     in SystemAPI.Query<DynamicBuffer<InputAim>, RefRO<DebugColor>>())
            {
                inputAims.GetDataAtTick(currentTick, out var inputAimAtTick);
                if (!inputAimAtTick.Set)
                {
                    //Log.Debug($"inputAimAtTick {currentTick} is not Set.");
                    continue;
                }
                
                var prefab = SystemAPI.GetSingleton<DebugAimInputPrefab>().Prefab;
                var e = state.EntityManager.Instantiate(prefab);
                state.EntityManager.SetComponentData(e, new LocalTransform
                {
                    Position = new float3(inputAimAtTick.MouseWorldPosition.x, 5.0f, inputAimAtTick.MouseWorldPosition.z),
                    Rotation = quaternion.identity,
                    Scale = 1.0f
                });
                    
                foreach (var child in SystemAPI.GetBuffer<LinkedEntityGroup>(e))
                {
                    if (!state.EntityManager.HasComponent<UnityEngine.SpriteRenderer>(child.Value))
                        continue;
                        
                    var spriteRenderer = state.EntityManager.GetComponentObject<UnityEngine.SpriteRenderer>(child.Value);
                    spriteRenderer.color = new UnityEngine.Color(debugColor.ValueRO.Value.x, debugColor.ValueRO.Value.y, debugColor.ValueRO.Value.z, 0.5f);
                }
            }
        }
    }
}
