using CEG.Gameplay.Client.Components;
using CEG.Gameplay.Shared.Components;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;

namespace CEG.Gameplay.Client.Systems
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct CameraFollowSystem : ISystem, ISystemStartStop
    {
        //[BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            Log.Verbose("OnCreate");
            state.RequireForUpdate<MainCameraTag>();
        }

        //[BurstCompile]
        public void OnStartRunning(ref SystemState state)
        {
            Log.Verbose("OnStartRunning");
        }

        //[BurstCompile]
        public void OnStopRunning(ref SystemState state)
        {
            Log.Verbose("OnStopRunning");
        }

        //[BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            Log.Verbose("OnDestroy");
        }
        
        public void OnUpdate(ref SystemState state)
        {
            var mainCameraEntity = SystemAPI.GetSingletonEntity<MainCameraTag>();
            var mainCamera = state.EntityManager.GetComponentObject<MainCamera>(mainCameraEntity);
            
            foreach(var (ltw, cf) in SystemAPI.Query<RefRO<LocalToWorld>, RefRO<CameraFollow>>().WithAll<GhostOwnerIsLocal>())
            {
                mainCamera.Camera.transform.position = ltw.ValueRO.Position + cf.ValueRO.Offset;
            }
        }
    }
}