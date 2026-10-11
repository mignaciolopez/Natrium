using CEG.Gameplay.Client.Components;
using CyberEntt.Logging;
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
        
        protected override void OnUpdate()
        {
            Log.Verbose("OnUpdate");
            Enabled = false;

            if (!SystemAPI.TryGetSingletonEntity<MainCameraTag>(out var mainCameraEntity))
            {
                Log.Error($"MainCameraTag not found: {mainCameraEntity}");
                return;
            }

            var mainCamera = EntityManager.GetComponentObject<MainCamera>(mainCameraEntity);
            if (!mainCamera.Camera)
            {
                Log.Warning($"No Camera specified on CameraAuthoring, setting up Camera.main on: {mainCameraEntity}");
                mainCamera.Camera = Camera.main;
            }
        }
    }
}