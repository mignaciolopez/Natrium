using CEG.Gameplay.Shared.Components;
using UnityEngine;
using Unity.Entities;
using Unity.NetCode;
using CEG.Gameplay.Shared.Components.Input;
using CEG.Settings.Input;
using CEG.Shared;
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
            
            foreach (var (localTransform, entity) in SystemAPI.Query<RefRO<LocalTransform>>()
                         .WithAll<GhostOwnerIsLocal>()
                         .WithEntityAccess())
            {
                _entityLocalPlayer = entity;
                break;
            }

            if (_entityLocalPlayer == Entity.Null)
            {
                Log.Error("Entity not found");
                Enabled = false;
            }
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