using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using Natrium.Shared;
using Natrium.Gameplay.Shared.Components;
using Natrium.Settings.Input;
using UnityEngine.InputSystem;

namespace Natrium.Gameplay.Client.Systems
{
    [UpdateInGroup(typeof(GhostInputSystemGroup))]
    public partial class AimInputSystem : SystemBase
    {
        private InputActions _inputActions;

        protected override void OnCreate()
        {
            _inputActions = new InputActions();
            RequireForUpdate<AimInput>();
        }

        protected override void OnStartRunning()
        {
            _inputActions.Enable();
            _inputActions.Map_Gameplay.Axn_MouseRealease.performed += OnPrimaryMouseRealease;
        }

        protected override void OnStopRunning()
        {
            _inputActions.Map_Gameplay.Axn_MouseRealease.performed -= OnPrimaryMouseRealease;
            _inputActions.Disable();
        }

        protected override void OnUpdate()
        {
            if (Camera.main == null)
            {
                return;
            }

            var newAimInput = new AimInput();

            if (_inputActions.Map_Gameplay.Axn_MouseRealease.WasPerformedThisFrame())
            {
                var mouseInputPosition = _inputActions.Map_Gameplay.Axn_MousePosition.ReadValue<Vector2>();
                var mousePosition = new Vector3(mouseInputPosition.x, mouseInputPosition.y, Camera.main.transform.position.y);
                var mouseWorldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

                Log.Debug($"mouseInputPosition {mouseInputPosition}, mousePosition {mousePosition}, mouseWorldPosition {mouseWorldPosition}");

                newAimInput.Value = mouseWorldPosition;
                newAimInput.Input.Set();
            }

            foreach (var ai in SystemAPI.Query<RefRW<AimInput>>())
            {
                ai.ValueRW = newAimInput;
            }
        }

        private void OnPrimaryMouseRealease(InputAction.CallbackContext context)
        {
            if (Camera.main == null)
            {
                Log.Error($"Camera.main == null");
            }
        }
    }
}