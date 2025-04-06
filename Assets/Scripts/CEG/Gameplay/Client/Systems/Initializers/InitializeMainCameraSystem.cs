using CEG.Gameplay.Client.Components;
using Unity.Entities;
using UnityEngine;

namespace CEG.Gameplay.Client.Systems.Initializers
{
    public partial class InitializeMainCameraSystem : SystemBase
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            Log.Verbose("OnCreate");
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            Log.Verbose("OnStartRunning");
        }

        protected override void OnStopRunning()
        {
            base.OnStopRunning();
            Log.Verbose("OnStopRunning");
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Log.Verbose("OnDestroy");
        }
        
        protected override void OnUpdate()
        {
            if (!SystemAPI.TryGetSingletonEntity<MainCameraTag>(out var mainCameraEntity))
            {
                Log.Error($"MainCameraTag not found: {mainCameraEntity}");
                return;
            }
            
            EntityManager.SetComponentData(mainCameraEntity, new MainCamera
            {
                Camera = Camera.main
            });
            Enabled = false;
        }
    }
}