using CEG.Logging;
using Gameplay.Client.Components;
using Unity.Entities;
using UnityEngine;

namespace Gameplay.Client.Systems.Initializers
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class InitializeMainCameraSystem : SystemBase
    {
        protected override void OnCreate()
        {
            Log.Verbose("OnCreate");
            if (SystemAPI.TryGetSingletonEntity<MainCameraTag>(out var entity))
                return;
            
            entity = EntityManager.CreateEntity();
            EntityManager.AddComponent<MainCameraTag>(entity);
            EntityManager.AddComponentObject(entity, new MainCamera
            {
                Camera = Camera.main
            });

            if (Camera.main != null)
            {
                Enabled = false;
            }
        }

        protected override void OnStartRunning()
        {
            Log.Verbose("OnStartRunning");
            
        }

        protected override void OnStopRunning()
        {
            Log.Verbose("OnStopRunning");
        }

        protected override void OnDestroy()
        {
            Log.Verbose("OnDestroy");
            if (!SystemAPI.TryGetSingletonEntity<MainCameraTag>(out var entity))
                return;
            
            var mainCamera = EntityManager.GetComponentObject<MainCamera>(entity);
            mainCamera.Camera = null;
            EntityManager.DestroyEntity(entity);
        }
        
        protected override void OnUpdate()
        {
            Log.Verbose("OnUpdate");

            if (!SystemAPI.TryGetSingletonEntity<MainCameraTag>(out var entity))
            {
                Log.Error($"{nameof(MainCameraTag)} not found");
                return;
            }

            var mainCamera = EntityManager.GetComponentObject<MainCamera>(entity);
            mainCamera.Camera = Camera.main;
            
            if (Camera.main != null)
            {
                Enabled = false;
            }
        }
    }
}