using CEG.Gameplay.Shared.Components;
using Unity.Entities;
using Unity.NetCode;
using CEG.Gameplay.Shared.Components.Input;
using CEG.Settings.Input;
using Unity.Collections;

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
            
            var entityQuery = SystemAPI.QueryBuilder()
                .WithAll<PlayerTag, GhostOwnerIsLocal>()
                .Build();

            var entities = entityQuery.ToEntityArray(Allocator.Temp);
            _entityLocalPlayer = entities.Length == 1 ? entities[0] : Entity.Null;

            if (_entityLocalPlayer == Entity.Null)
            {
                Log.Error($"Failed to obtain just one entity with {nameof(PlayerTag)} and {nameof(GhostOwnerIsLocal)} " +
                          $"entities.Length: {entities.Length}");

                Enabled = false;
            }
            
            entities.Dispose();
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