using Unity.Entities;
using Unity.Transforms;
using Unity.NetCode;
using UnityEngine;

namespace Natrium
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial class CameraSystem : SystemBase
    {
        private Camera mCurrentCamera;
        protected override void OnCreate()
        {
            base.OnCreate();
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();

            mCurrentCamera = Camera.main;
        }

        protected override void OnStopRunning()
        {
            base.OnStopRunning();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
        protected override void OnUpdate()
        {
            foreach(var (ltw, cd, pd, pid) in SystemAPI.Query<LocalToWorld, CameraData, PlayerData, PlayerInputData>().WithAll<Simulate>())
            {
                mCurrentCamera.transform.position = ltw.Position + cd.offset;
            }
        }
    }
}