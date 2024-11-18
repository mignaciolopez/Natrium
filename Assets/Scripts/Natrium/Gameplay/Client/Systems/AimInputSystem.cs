using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using Natrium.Shared;
using Natrium.Gameplay.Shared.Components;
using Natrium.Settings.Input;
using Unity.Mathematics;
using UnityEngine.InputSystem;

namespace Natrium.Gameplay.Client.Systems
{
    [UpdateInGroup(typeof(GhostInputSystemGroup))] //This group only executes on Client.
    public partial class AimInputSystem : SystemBase
    {
        private InputActions _inputActions;

        protected override void OnCreate()
        {
            base.OnCreate();
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnCreate()");
            _inputActions = new InputActions();
            RequireForUpdate<GhostOwnerIsLocal>();
            RequireForUpdate<AimInput>();
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnStartRunning()");
            _inputActions.Enable();
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
            foreach (var aimInput in SystemAPI.Query<RefRW<AimInput>>().WithAll<GhostOwnerIsLocal>())
            {
                //Manually reset AimInputEvent due to wrong documentation.
                //It is not being reset after being processed.
                //https://discussions.unity.com/t/inputevent-does-not-fire-exactly-once/929531/3
                aimInput.ValueRW.AimInputEvent = default;
                
                if (_inputActions.Map_Gameplay.Axn_MouseRealease.WasPerformedThisFrame())
                {
                    Log.Verbose($"[{World.Name}] | OnPrimaryMouseRelease()");

                    if (Camera.main == null)
                    {
                        Log.Error($"[{World.Name}] | Camera.main is null.");
                        return;
                    }
                        
                    var mouseInputPosition = _inputActions.Map_Gameplay.Axn_MousePosition.ReadValue<Vector2>();
                    var mousePosition = new Vector3(mouseInputPosition.x, mouseInputPosition.y, Camera.main.transform.position.y);
                    var mouseWorldPosition = (float3)Camera.main.ScreenToWorldPoint(mousePosition);
            
                    Log.Debug($"mouseInputPosition: {mouseInputPosition}\n" +
                              $"mousePosition: {mousePosition}\n" +
                              $"mouseWorldPosition: {mouseWorldPosition}");
                        
                    aimInput.ValueRW.AimInputEvent.Set();
                    aimInput.ValueRW.Value = mouseWorldPosition;
                }
            }
        }
    }
}