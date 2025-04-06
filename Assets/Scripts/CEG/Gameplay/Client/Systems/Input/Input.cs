using CEG.Gameplay.Client.Components;
using CEG.Gameplay.Shared.Components;
using CEG.Settings.Input;
using Unity.Entities;
using UnityEngine;

namespace CEG.Gameplay.Client.Systems.Input
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateBefore(typeof(ClientSystem))]
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
            var ecb = new EntityCommandBuffer(WorldUpdateAllocator);
            
            //Todo: Move all the following stuff to another system.
            if (UnityEngine.Input.GetKeyUp(KeyCode.Return))
            {
                var entity = ecb.CreateEntity();
                ecb.AddComponent<ConnectRequest>(entity);
            }

            if (UnityEngine.Input.GetKeyUp(KeyCode.Escape))
            {
                var entity = ecb.CreateEntity();
                ecb.AddComponent<DisconnectRequest>(entity);
            }

            if (UnityEngine.Input.GetKeyUp(KeyCode.F11))
            {
                if (Screen.fullScreen)
                    Screen.fullScreenMode = FullScreenMode.Windowed;
                else
                    Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
            }

            if (UnityEngine.Input.GetKeyUp(KeyCode.F2))
            {
                var entity = ecb.CreateEntity();
                ecb.AddComponent<PingRequest>(entity);
            }
            //End //Todo: Move all the following stuff to another system.
            
            ecb.Playback(EntityManager);
        }
    }
}