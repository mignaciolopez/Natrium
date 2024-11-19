using Natrium.Gameplay.Client.Components;
using Natrium.Shared;
using Unity.Entities;
using UnityEngine;

namespace Natrium.Gameplay.Client.Systems.Initializers
{
    public partial class InitializeMainCameraSystem : SystemBase
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnCreate()");
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnStartRunning()");
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