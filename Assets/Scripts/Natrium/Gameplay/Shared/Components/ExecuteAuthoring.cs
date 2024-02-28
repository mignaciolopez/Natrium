using Unity.Entities;
using UnityEngine;

namespace Natrium.Gameplay.Shared.Components
{
    public class ExecuteAuthoring : MonoBehaviour
    {
        [Header("Shared")]
        #region Shared
        public bool AimSystem = true;
        public bool MeeleSystem = true;
        public bool MovementSystem = true;
        public bool PingPongSystem = true;
        #endregion //Shared

        [Header("Client")]
        #region Client
        public bool CameraSystem = true;
        public bool ClientSystem = true;
        public bool InputSystem = true;
        public bool PlayerNameSystem = true;
        public bool DebugTileSystem = true;
        #endregion //Client

        [Header("Server")]
        #region Server
        public bool AttackSystem = true;
        public bool RaycastSystem = true;
        public bool ServerSystem = true;
        #endregion //Server
    }

    public class ExecuteBaker : Baker<ExecuteAuthoring>
    {
        public override void Bake(ExecuteAuthoring authoring)
        {
            var e = GetEntity(TransformUsageFlags.None);

            #region Shared
            if (authoring.AimSystem)
                AddComponent<AimSystemExecute>(e);

            if (authoring.MeeleSystem)
                AddComponent<MeeleSystemExecute>(e);

            if (authoring.MovementSystem)
                AddComponent<MovementSystemExecute>(e);

            if (authoring.PingPongSystem)
                AddComponent<PingPongSystemExecute>(e);
            #endregion Shared

            #region Client
            if (authoring.CameraSystem)
                AddComponent<CameraSystemExecute>(e);

            if (authoring.ClientSystem)
                AddComponent<ClientSystemExecute>(e);

            if (authoring.InputSystem)
                AddComponent<InputSystemExecute>(e);

            if (authoring.PlayerNameSystem)
                AddComponent<PlayerNameSystemExecute>(e);

            if (authoring.DebugTileSystem)
                AddComponent<DebugTileSystemExecute>(e);

            #endregion Client

            #region Server
            if (authoring.AttackSystem)
                AddComponent<AttackSystemExecute>(e);

            if (authoring.RaycastSystem)
                AddComponent<RaycastSystemExecute>(e);

            if (authoring.ServerSystem)
                AddComponent<ServerSystemExecute>(e);

            #endregion Server
        }
    }

    #region Shared
    public struct AimSystemExecute : IComponentData {}
    public struct MeeleSystemExecute : IComponentData {}
    public struct MovementSystemExecute : IComponentData {}
    public struct PingPongSystemExecute : IComponentData {}
    #endregion Shared

    #region Client
    public struct CameraSystemExecute : IComponentData {}
    public struct ClientSystemExecute : IComponentData {}
    public struct InputSystemExecute : IComponentData {}
    public struct PlayerNameSystemExecute : IComponentData {}
    public struct DebugTileSystemExecute : IComponentData {}
    #endregion Client

    #region Server
    public struct AttackSystemExecute : IComponentData {}
    public struct RaycastSystemExecute : IComponentData {}
    public struct ServerSystemExecute : IComponentData {}
    #endregion Server
}
