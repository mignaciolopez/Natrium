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
    public partial class MeeleInputSystem : SystemBase
    {
        private InputActions _inputActions;

        protected override void OnCreate()
        {
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnCreate()");
            _inputActions = new InputActions();
            RequireForUpdate<MeeleInput>();
        }

        protected override void OnStartRunning()
        {
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnStartRunning()");
            _inputActions.Enable();
            
        }

        protected override void OnStopRunning()
        {
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnStopRunning()");
            _inputActions.Disable();
        }

        protected override void OnDestroy()
        {
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnDestroy()");
            base.OnDestroy();
        }
        
        protected override void OnUpdate()
        {
            //Use this approach on Update or use OnPrimaryMouseRealease but not both
            /*

            */
        }
    }
}