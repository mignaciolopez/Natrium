using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using Natrium.Shared;
using Natrium.Gameplay.Shared.Components;
using Natrium.Settings.Input;
using UnityEngine.InputSystem;

namespace Natrium.Gameplay.Client.Systems
{    
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class AimSystem : SystemBase
    {
        private InputActions _inputActions;

        protected override void OnCreate()
        {
            _inputActions = new InputActions();
            RequireForUpdate<NetworkStreamInGame>();
        }

        protected override void OnStartRunning()
        {
            _inputActions.Enable();
            _inputActions.Map_Gameplay.Axn_MouseRealease.performed += OnPrimaryClick;
        }

        protected override void OnStopRunning()
        {
            _inputActions.Map_Gameplay.Axn_MouseRealease.performed -= OnPrimaryClick;
            _inputActions.Disable();
        }

        protected override void OnUpdate()
        {
            
        }

        private void OnPrimaryClick(InputAction.CallbackContext context)
        {
            var ecb = new EntityCommandBuffer(WorldUpdateAllocator);

            if (Camera.main != null)
            {
                var mouseInputPosition = _inputActions.Map_Gameplay.Axn_MousePosition.ReadValue<Vector2>();
                var mousePosition = new Vector3(mouseInputPosition.x, mouseInputPosition.y, Camera.main.transform.position.y);
                var mouseWorldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

                Log.Debug($"mouseInputPosition {mouseInputPosition}");
                Log.Debug($"mousePosition {mousePosition}");
                Log.Debug($"mouseWorldPosition {mouseWorldPosition}");

                var req = ecb.CreateEntity();
                ecb.AddComponent(req, new RpcAim { MouseWorldPosition = mouseWorldPosition });
                ecb.AddComponent<SendRpcCommandRequest>(req);
            }

            ecb.Playback(EntityManager);
        }
    }
}