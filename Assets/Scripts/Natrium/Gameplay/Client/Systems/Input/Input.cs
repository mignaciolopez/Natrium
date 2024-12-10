using Natrium.Settings.Input;
using Natrium.Shared;
using Natrium.Shared.Systems;
using Unity.Entities;
using UnityEngine;

namespace Natrium.Gameplay.Client.Systems.Input
{
    public partial class Input : SystemBase
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
            _inputActions.Dispose();
        }
        protected override void OnUpdate()
        {
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