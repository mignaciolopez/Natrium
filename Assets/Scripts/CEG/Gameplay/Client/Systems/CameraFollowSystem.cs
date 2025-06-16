using CEG.Gameplay.Client.Components;
using CEG.Gameplay.Shared.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

namespace CEG.Gameplay.Client.Systems
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct CameraFollowSystem : ISystem, ISystemStartStop
    {
        //[BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            Log.Verbose("OnCreate");
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
            SystemAPI.TryGetSingletonEntity<MainCameraTag>(out var mainCameraEntity);
            if (mainCameraEntity == Entity.Null)
            {
                Log.Error($"Need {nameof(MainCameraTag)} to work.");
                return;
            }
            
            var mainCamera = state.EntityManager.GetComponentObject<MainCamera>(mainCameraEntity);
            
            foreach(var (ltw, cf) in SystemAPI.Query<RefRO<LocalToWorld>, RefRW<CameraFollow>>().WithAll<GhostOwnerIsLocal>())
            {
                if (!cf.ValueRO.OverrideSceneSettings)
                {
                    cf.ValueRW.Offset = (float3)mainCamera.Camera.transform.position - ltw.ValueRO.Position;
                    cf.ValueRW.OverrideSceneSettings = true;
                }

                mainCamera.Camera.transform.position = ltw.ValueRO.Position + cf.ValueRO.Offset;
            }
        }
    }
}