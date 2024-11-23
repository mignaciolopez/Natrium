using System.Globalization;
using Natrium.Gameplay.Client.Components;
using Natrium.Gameplay.Shared.Components;
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
        private Entity _entityLocalPlayer;
        protected override void OnCreate()
        {
            base.OnCreate();
            Log.Verbose("OnCreate");
            _inputActions = new InputActions();
            RequireForUpdate<GhostOwnerIsLocal>();
            RequireForUpdate<PlayerTag>();
            RequireForUpdate<InputAim>();
            RequireForUpdate<NetworkTime>();
            RequireForUpdate<MainCameraTag>();
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            Log.Verbose("OnStartRunning");
            _inputActions.Enable();
            var mainCameraEntity = SystemAPI.GetSingletonEntity<MainCameraTag>();
            _mainCamera = EntityManager.GetComponentObject<MainCamera>(mainCameraEntity);
            foreach (var (playerTag, entity) in SystemAPI.Query<RefRO<PlayerTag>>()
                         .WithAll<PlayerTag, GhostOwnerIsLocal>().WithEntityAccess())
            {
                _entityLocalPlayer = entity;
            }
        }

        protected override void OnStopRunning()
        {
            base.OnStopRunning();
            Log.Verbose("OnStopRunning");
            _inputActions.Disable();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Log.Verbose("OnDestroy");
            _inputActions.Dispose();
        }

        protected override void OnUpdate()
        {
            var currentTick = SystemAPI.GetSingleton<NetworkTime>().InterpolationTick;
            var inputAim = EntityManager.GetBuffer<InputAim>(_entityLocalPlayer);

            var mouseInputPosition = _inputActions.Map_Gameplay.Axn_MousePosition.ReadValue<Vector2>();
            var mousePosition = new Vector3(mouseInputPosition.x, mouseInputPosition.y, _mainCamera.Camera.transform.position.y);
            var mouseWorldPosition = (float3)_mainCamera.Camera.ScreenToWorldPoint(mousePosition);

            inputAim.AddCommandData(new InputAim
            {
                Tick = currentTick,
                Set = _inputActions.Map_Gameplay.Axn_MouseRealease.WasPerformedThisFrame(),
                MouseWorldPosition = mouseWorldPosition
            });
                
            if (_inputActions.Map_Gameplay.Axn_MouseRealease.WasPerformedThisFrame())
            {
                Log.Verbose("OnPrimaryMouseRelease");
                
                Log.Debug($"mouseWorldPosition: {mouseWorldPosition.ToString("F2", CultureInfo.InvariantCulture)}\n" +
                          $"mouseInputPosition: {mouseInputPosition}\n" + 
                          $"mousePosition: {mousePosition}\n");
            }
        }
    }
}