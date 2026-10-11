using CEG.Gameplay.Client.Components;
using CEG.Gameplay.Shared.Components;
using CyberEntt.Logging;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;

namespace CEG.Gameplay.Client.Systems
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct CameraFollowSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            SystemAPI.TryGetSingletonEntity<MainCameraTag>(out var mainCameraEntity);
            if (mainCameraEntity == Entity.Null)
            {
                Log.Error($"Need {nameof(MainCameraTag)} to work.");
                return;
            }
            
            var mainCamera = state.EntityManager.GetComponentObject<MainCamera>(mainCameraEntity);
            foreach(var (ltw, cf) in SystemAPI.Query<LocalToWorld, CameraFollow>()
                        .WithAll<GhostOwnerIsLocal>())
            {
                mainCamera.Camera.transform.position = ltw.Position + cf.Offset;
            }
        }
    }
}