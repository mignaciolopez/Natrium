using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Natrium
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
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
            foreach((LocalToWorld ltw, CameraData cd) in SystemAPI.Query<LocalToWorld, CameraData>())
            {
                mCurrentCamera.transform.position = ltw.Position + cd.offset;
            }
        }
    }
}