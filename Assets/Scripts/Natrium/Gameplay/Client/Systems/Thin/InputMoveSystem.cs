using Natrium.Gameplay.Shared.Components;
using UnityEngine;
using Unity.Entities;
using Unity.NetCode;
using Natrium.Gameplay.Shared.Components.Input;
using Natrium.Settings.Input;
using Natrium.Shared;
using Natrium.Shared.Systems;

namespace Natrium.Gameplay.Client.Systems.Thin
{
    [UpdateInGroup(typeof(GhostInputSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ThinClientSimulation)]
    public partial class InputMoveSystem : SystemBase
    {
        private InputActions _inputActions;

        protected override void OnCreate()
        {
            Log.Verbose("OnCreate");
            _inputActions = new InputActions();
        }

        protected override void OnStartRunning()
        {
            Log.Verbose("OnStartRunning");
            _inputActions.Enable();
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
            Log.Verbose("OnUpdate");
            foreach (var inputAxis in SystemAPI.Query<RefRW<InputMove>>())
            {
                inputAxis.ValueRW.Value = _inputActions.Map_Gameplay.Axn_PlayerMove.ReadValue<Vector2>();
            }
        }
    }
}