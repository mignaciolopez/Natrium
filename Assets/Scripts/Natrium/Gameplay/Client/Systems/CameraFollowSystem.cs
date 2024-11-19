using Unity.Entities;
using Unity.Transforms;
using Natrium.Gameplay.Client.Components;
using Natrium.Shared;
using Unity.NetCode;

namespace Natrium.Gameplay.Client.Systems
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct CameraFollowSystem : ISystem, ISystemStartStop
    {
        //[BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] | {this.ToString()}.OnCreate()");
            state.RequireForUpdate<MainCameraTag>();
        }

        //[BurstCompile]
        public void OnStartRunning(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] | {this.ToString()}.OnStartRunning()");
        }

        //[BurstCompile]
        public void OnStopRunning(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] | {this.ToString()}.OnStopRunning()");
        }

        //[BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] | {this.ToString()}.OnDestroy()");
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