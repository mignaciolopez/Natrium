using Natrium.Gameplay.Shared.Components;
using UnityEngine;
using Unity.Entities;
using Unity.NetCode;
using Natrium.Gameplay.Shared.Components.Input;
using Natrium.Settings.Input;
using Natrium.Shared;
using Natrium.Shared.Systems;
using Unity.Mathematics;

namespace Natrium.Gameplay.Client.Systems.Input
{
    [UpdateInGroup(typeof(GhostInputSystemGroup))]
    public partial class InputMoveSystem : SystemBase
    {
        private InputActions _inputActions;

        protected override void OnCreate()
        {
            Log.Verbose("OnCreate");
            _inputActions = new InputActions();
            var required = SystemAPI.QueryBuilder().WithAll<InputMove, GhostOwnerIsLocal, Simulate>().Build();
            //RequireForUpdate(required);
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
            foreach (var (inputAxis, entity) in SystemAPI.Query<RefRW<InputMove>>()
                         .WithAll<GhostOwnerIsLocal, Simulate>()
                         .WithEntityAccess())
            {
                inputAxis.ValueRW.Value = _inputActions.Map_Gameplay.Axn_PlayerMove.ReadValue<Vector2>();
            }

            
            //Todo: Move all the following stuff to another system.
            if (UnityEngine.Input.GetKeyUp(KeyCode.Return))
                EventSystem.EnqueueEvent(Natrium.Shared.Events.OnKeyCodeReturn);
            if (UnityEngine.Input.GetKeyUp(KeyCode.Escape))
                EventSystem.EnqueueEvent(Natrium.Shared.Events.OnKeyCodeEscape);

            if (UnityEngine.Input.GetKeyUp(KeyCode.F11))
            {
                if (Screen.fullScreen)
                    Screen.fullScreenMode = FullScreenMode.Windowed;
                else
                    Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
            }

            if (UnityEngine.Input.GetKeyUp(KeyCode.F2))
            {
                EventSystem.EnqueueEvent(Natrium.Shared.Events.OnSendPing);
            }
            //End //Todo: Move all the following stuff to another system.
        }
    }
}