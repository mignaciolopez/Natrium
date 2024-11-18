using Natrium.Gameplay.Client.Components.UI;
using Natrium.Gameplay.Shared.Components;
using Natrium.Shared;
using Unity.Entities;
using Unity.NetCode;
using Unity.Mathematics;
using Unity.Transforms;

namespace Natrium.Gameplay.Client.Systems.UI
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class DebugAimSystem : SystemBase
    {
        private BeginInitializationEntityCommandBufferSystem.Singleton _bsEcbS;
        
        protected override void OnCreate()
        {
            base.OnCreate();
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnCreate()");
            RequireForUpdate<NetworkTime>();
            RequireForUpdate<AimInput>();
            RequireForUpdate<DebugColor>();
            RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnStartRunning()");
            
            _bsEcbS = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
        }

        protected override void OnStopRunning()
        {
            base.OnStopRunning();
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnStopRunning()");
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnDestroy()");
        }
        
        protected override void OnUpdate()
        {
            QueryAndShowAimInputs();
        }

        private void QueryAndShowAimInputs()
        {
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();
            if (!networkTime.IsFirstTimeFullyPredictingTick) return;
            
            foreach (var (aimInput, debugColor) in SystemAPI.Query<RefRO<AimInput>, RefRO<DebugColor>>())
            {
                if (aimInput.ValueRO.AimInputEvent.IsSet)
                {
                    var prefab = SystemAPI.GetSingleton<DebugAimInputPrefab>().Prefab;
                    var e = EntityManager.Instantiate(prefab);
                    EntityManager.SetComponentData(e, new LocalTransform
                    {
                        Position = new float3(aimInput.ValueRO.Value.x, 5.0f, aimInput.ValueRO.Value.z),
                        Rotation = quaternion.identity,
                        Scale = 1.0f
                    });
                    
                    foreach (var child in SystemAPI.GetBuffer<LinkedEntityGroup>(e))
                    {
                        if (!EntityManager.HasComponent<UnityEngine.SpriteRenderer>(child.Value)) continue;
                        
                        var spriteRenderer = EntityManager.GetComponentObject<UnityEngine.SpriteRenderer>(child.Value);
                        spriteRenderer.color = new UnityEngine.Color(debugColor.ValueRO.Value.x, debugColor.ValueRO.Value.y, debugColor.ValueRO.Value.z, 0.5f);
                    }
                }
            }
        }
    }
}
