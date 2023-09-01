using Natrium.Ecs.Components;
using Natrium.Ecs.Systems;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Natrium.ECS.Systems
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial class CameraSystem : SystemBase
    {
        private Camera _mCurrentCamera;
        protected override void OnCreate()
        {
            
            base.OnCreate();
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();

            _mCurrentCamera = Camera.main;
            EventSystem.DispatchEvent(Events.OnResolutionChange);
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
            foreach( var (ltw, cd) in SystemAPI.Query<LocalToWorld, CameraOffset>())
            {
                _mCurrentCamera.transform.position = ltw.Position + cd.Value;
            }
        }
    }
}