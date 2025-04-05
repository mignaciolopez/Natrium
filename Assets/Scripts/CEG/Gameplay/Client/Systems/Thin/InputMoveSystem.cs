using CEG.Gameplay.Shared.Components;
using UnityEngine;
using Unity.Entities;
using Unity.NetCode;
using CEG.Gameplay.Shared.Components.Input;
using CEG.Settings.Input;
using CEG.Shared;
using CEG.Shared.Systems;

namespace CEG.Gameplay.Client.Systems.Thin
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

        }
    }
}