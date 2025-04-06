using System.Globalization;
using CEG.Gameplay.Client.Components;
using CEG.Gameplay.Shared.Components;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using CEG.Gameplay.Shared.Components.Input;
using CEG.Settings.Input;
using Unity.Collections;

namespace CEG.Gameplay.Client.Systems.Input
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
            
            var entityQuery = SystemAPI.QueryBuilder()
                .WithAll<PlayerTag, GhostOwnerIsLocal>()
                .Build();

            var entities = entityQuery.ToEntityArray(Allocator.Temp);
            _entityLocalPlayer = entities.Length == 1 ? entities[0] : Entity.Null;

            if (_entityLocalPlayer == Entity.Null)
            {
                Log.Error($"Failed to obtain just one entity with {nameof(PlayerTag)} and {nameof(GhostOwnerIsLocal)} " +
                          $"entities.Length: {entities.Length}");

                Enabled = false;
            }
            
            entities.Dispose();
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
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();
            var inputAim = SystemAPI.GetComponentRW<InputAim>(_entityLocalPlayer);

            inputAim.ValueRW.ServerTick = networkTime.ServerTick;
            inputAim.ValueRW.InputEvent = default;

            if (_inputActions.Map_Gameplay.Axn_MouseRealease.WasReleasedThisFrame())
            {
                var mouseInputPosition = _inputActions.Map_Gameplay.Axn_MousePosition.ReadValue<Vector2>();
                var ray = _mainCamera.Camera.ScreenPointToRay(mouseInputPosition);
                inputAim.ValueRW.InputEvent.Set();
                inputAim.ValueRW.Origin = ray.origin;
                inputAim.ValueRW.Direction = ray.direction;
                
                Log.Debug($"OnPrimaryMouseRelease@{networkTime.ServerTick} | mouseInputPosition: {mouseInputPosition.ToString("F2", CultureInfo.InvariantCulture)}");
            }
        }
    }
}