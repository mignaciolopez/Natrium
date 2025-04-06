using CEG.Gameplay.Shared.Components;
using CEG.Gameplay.Shared.Components.Input;
using CEG.Settings.Input;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;

namespace CEG.Gameplay.Client.Systems.Input
{
    [UpdateInGroup(typeof(GhostInputSystemGroup))]
    public partial class InputMoveSystem : SystemBase
    {
        private InputActions _inputActions;
        private Entity _entityLocalPlayer;

        protected override void OnCreate()
        {
            Log.Verbose("OnCreate");
            _inputActions = new InputActions();

            RequireForUpdate<LocalTransform>();
            RequireForUpdate<GhostOwnerIsLocal>();
            RequireForUpdate<PlayerTag>();
        }

        protected override void OnStartRunning()
        {
            Log.Verbose("OnStartRunning");
            _inputActions.Enable();
            
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
            Log.Verbose("OnStopRunning");
            _inputActions.Disable();
        }

        protected override void OnDestroy()
        {
            Log.Verbose("OnDestroy");
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            var inputMove = SystemAPI.GetComponentRW<InputMove>(_entityLocalPlayer);
            inputMove.ValueRW.Value = _inputActions.Map_Gameplay.Axn_PlayerMove.ReadValue<Vector2>();
        }
    }
}