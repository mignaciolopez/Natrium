using Natrium.Gameplay.Client.Components;
using Natrium.Gameplay.Shared.Components;
using Unity.Entities;
using Unity.NetCode;
using Natrium.Shared;
using Natrium.Gameplay.Shared.Components.Input;
using Natrium.Settings.Input;

namespace Natrium.Gameplay.Client.Systems.Input
{
    [UpdateInGroup(typeof(GhostInputSystemGroup))] //This group only executes on Client.
    public partial class InputMeleeSystem : SystemBase
    {
        private InputActions _inputActions;
        private Entity _entityLocalPlayer;
        protected override void OnCreate()
        {
            base.OnCreate();
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnCreate()");
            _inputActions = new InputActions();
            RequireForUpdate<GhostOwnerIsLocal>();
            RequireForUpdate<PlayerTag>();
            RequireForUpdate<InputMelee>();
            RequireForUpdate<NetworkTime>();
            RequireForUpdate<MainCameraTag>();
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnStartRunning()");
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
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnStopRunning()");
            _inputActions.Disable();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnDestroy()");
            _inputActions.Dispose();
        }

        protected override void OnUpdate()
        {
            var currentTick = SystemAPI.GetSingleton<NetworkTime>().InterpolationTick;
            var inputMelee = EntityManager.GetBuffer<InputMelee>(_entityLocalPlayer);

            inputMelee.AddCommandData(new InputMelee
            {
                Tick = currentTick,
                Set = _inputActions.Map_Gameplay.Axn_MeleePrimary.WasPerformedThisFrame()
            });
                
            if (_inputActions.Map_Gameplay.Axn_MeleePrimary.WasPerformedThisFrame())
            {
                Log.Verbose($"[{World.Name}] | OnPrimaryMeleeAction");
            }
        }
    }
}