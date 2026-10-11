using CEG.Gameplay.Client.Components;
using CEG.Gameplay.Shared.Components;
using CyberEntt.Logging;
using Unity.Entities;
using UnityEngine;

namespace CEG.Gameplay.Client.Systems.Input
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateBefore(typeof(ClientSystem))]
    public partial class InputSystem : SystemBase
    {
        private Settings.Input.Actions _actions;
        
        protected override void OnCreate()
        {
            Log.Verbose("OnCreate");
            _actions = new Settings.Input.Actions();
        }

        protected override void OnStartRunning()
        {
            Log.Verbose("OnStartRunning");
            _actions.Enable();
        }

        protected override void OnStopRunning()
        {
            Log.Verbose("OnStopRunning");
            _actions.Disable();
        }

        protected override void OnDestroy()
        {
            Log.Verbose("OnDestroy");
            base.OnDestroy();
            _actions.Dispose();
        }
        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(WorldUpdateAllocator);
            
            if (_actions.Login.Enter.WasPerformedThisFrame())
            {
                var entity = ecb.CreateEntity();
                ecb.AddComponent<ConnectRequest>(entity);
            }

            if (_actions.Login.Escape.WasPerformedThisFrame())
            {
                var entity = ecb.CreateEntity();
                ecb.AddComponent<DisconnectRequest>(entity);
            }

            if (_actions.Gameplay.ToggleFullScreen.WasPerformedThisFrame())
            {
                if (Screen.fullScreen)
                    Screen.fullScreenMode = FullScreenMode.Windowed;
                else
                    Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
            }

            if (_actions.Gameplay.Ping.WasPerformedThisFrame())
            {
                var entity = ecb.CreateEntity();
                ecb.AddComponent<PingRequest>(entity);
            }
            
            ecb.Playback(EntityManager);
        }
    }
}