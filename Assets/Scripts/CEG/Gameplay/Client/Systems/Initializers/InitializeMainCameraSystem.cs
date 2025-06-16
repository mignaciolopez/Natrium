using CEG.Gameplay.Client.Components;
using Unity.Entities;
using UnityEngine;

namespace CEG.Gameplay.Client.Systems.Initializers
{
    public partial class InitializeMainCameraSystem : SystemBase
    {
        protected override void OnCreate()
        {
            Log.Verbose("OnCreate");
        }

        protected override void OnStartRunning()
        {
            Log.Verbose("OnStartRunning");
            Enabled = false;
            
            if (!SystemAPI.TryGetSingletonEntity<MainCameraTag>(out var mainCameraEntity))
            {
                Log.Error($"MainCameraTag not found: {mainCameraEntity}");
                return;
            }

            var mainCamera = EntityManager.GetComponentObject<MainCamera>(mainCameraEntity);
            if (!mainCamera.Camera)
            {
                Log.Info($"No Camera specified on CameraAuthoring, setting up Camera.main on: {mainCameraEntity}");
                mainCamera.Camera = Camera.main;
            }
        }

        protected override void OnStopRunning()
        {
            Log.Verbose("OnStopRunning");
        }

        protected override void OnDestroy()
        {
            Log.Verbose("OnDestroy");
        }
        
        protected override void OnUpdate()
        {
            
        }
    }
}