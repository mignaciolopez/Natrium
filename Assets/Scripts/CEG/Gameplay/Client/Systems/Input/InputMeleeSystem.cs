using CEG.Gameplay.Client.Components;
using CEG.Gameplay.Shared.Components;
using Unity.Entities;
using Unity.NetCode;
using CEG.Shared;
using CEG.Gameplay.Shared.Components.Input;
using CEG.Settings.Input;

namespace CEG.Gameplay.Client.Systems.Input
{
    [UpdateInGroup(typeof(GhostInputSystemGroup))] //This group only executes on Client.
    public partial class InputMeleeSystem : SystemBase
    {
        private InputActions _inputActions;
        private Entity _entityLocalPlayer;
        protected override void OnCreate()
        {
            base.OnCreate();
            Log.Verbose("OnCreate");
            _inputActions = new InputActions();
            RequireForUpdate<GhostOwnerIsLocal>();
            RequireForUpdate<PlayerTag>();
            RequireForUpdate<InputMelee>();
            RequireForUpdate<NetworkTime>();
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            Log.Verbose("OnStartRunning");
            _inputActions.Enable();
            var mainCameraEntity = SystemAPI.GetSingletonEntity<MainCameraTag>();
            
            foreach (var (playerTag, entity) in SystemAPI.Query<RefRO<PlayerTag>>()
                         .WithAll<PlayerTag, GhostOwnerIsLocal>().WithEntityAccess())
            {
                _entityLocalPlayer = entity;
            }
        }

        protected override void OnStopRunning()
        {
            base.OnStopRunning();
            Log.Verbose("OnStopRunning");
            _inputActions.Disable();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Log.Verbose("OnDestroy");
            _inputActions.Dispose();
        }

        protected override void OnUpdate()
        {
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();
            var inputMelee = SystemAPI.GetComponentRW<InputMelee>(_entityLocalPlayer);
            
            inputMelee.ValueRW.ServerTick = networkTime.ServerTick;
            inputMelee.ValueRW.InputEvent = default;

            if (_inputActions.Map_Gameplay.Axn_MeleePrimary.WasPerformedThisFrame())
            {
                inputMelee.ValueRW.InputEvent.Set();
                Log.Verbose($"OnPrimaryMeleeAction@{networkTime.ServerTick}");
            }
        }
    }
}