using System.Globalization;
using Natrium.Gameplay.Client.Components;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using Natrium.Shared;
using Natrium.Gameplay.Shared.Components.Input;
using Natrium.Settings.Input;
using Unity.Mathematics;

namespace Natrium.Gameplay.Client.Systems.Input
{
    [UpdateInGroup(typeof(GhostInputSystemGroup))] //This group only executes on Client.
    public partial class InputAimSystem : SystemBase
    {
        private InputActions _inputActions;
        private MainCamera _mainCamera;
        protected override void OnCreate()
        {
            base.OnCreate();
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnCreate()");
            _inputActions = new InputActions();
            RequireForUpdate<GhostOwnerIsLocal>();
            RequireForUpdate<InputAim>();
            RequireForUpdate<NetworkTime>();
            RequireForUpdate<MainCameraTag>();
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnStartRunning()");
            _inputActions.Enable();
            var mainCameraEntity = SystemAPI.GetSingletonEntity<MainCameraTag>();
            _mainCamera = EntityManager.GetComponentObject<MainCamera>(mainCameraEntity);
        }

        protected override void OnStopRunning()
        {
            base.OnStopRunning();
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnStopRunning()");
            _inputActions.Disable();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnDestroy()");
            _inputActions.Dispose();
        }

        protected override void OnUpdate()
        {
            var currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
            if (!currentTick.IsValid)
            {
                Log.Warning($"currentTick is Invalid!");
                return;
            }

            foreach (var inputAim in SystemAPI.Query<DynamicBuffer<InputAim>>().WithAll<GhostOwnerIsLocal>())
            {
                inputAim.AddCommandData(new InputAim
                {
                    Tick = currentTick,
                    Set = false,
                });

                if (!_inputActions.Map_Gameplay.Axn_MouseRealease.WasPerformedThisFrame())
                    continue;
                
                Log.Verbose($"[{World.Name}] | OnPrimaryMouseRelease()");
                    
                var mouseInputPosition = _inputActions.Map_Gameplay.Axn_MousePosition.ReadValue<Vector2>();
                var mousePosition = new Vector3(mouseInputPosition.x, mouseInputPosition.y, _mainCamera.Camera.transform.position.y);
                var mouseWorldPosition = (float3)_mainCamera.Camera.ScreenToWorldPoint(mousePosition);
            
                Log.Debug($"mouseWorldPosition: {mouseWorldPosition.ToString("F2", CultureInfo.InvariantCulture)}\n" +
                          $"mouseInputPosition: {mouseInputPosition}\n" + 
                          $"mousePosition: {mousePosition}\n");

                inputAim.AddCommandData(new InputAim
                {
                    Tick = currentTick,
                    Set = true,
                    MouseWorldPosition = mouseWorldPosition
                });
            }
        }
    }
}