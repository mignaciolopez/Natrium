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
            _inputActions = new InputActions();
            RequireForUpdate<MeeleInput>();
        }

        protected override void OnStartRunning()
        {
            _inputActions.Enable();
            
        }

        protected override void OnStopRunning()
        {
            
            _inputActions.Disable();
        }

        protected override void OnUpdate()
        {
            //Use this approach on Update or use OnPrimaryMouseRealease but not both
            /*

            */
        }
    }
}