using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Natrium.Gameplay.Client.Components;
using Unity.NetCode;

namespace Natrium.Gameplay.Client.Systems
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class CameraSystem : SystemBase
    {
        private Camera _currentCamera;

        protected override void OnCreate()
        {
            base.OnCreate();
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();

            _currentCamera = Camera.main;
        }
        
        protected override void OnUpdate()
        {
            foreach(var (ltw, cd) in SystemAPI.Query<LocalToWorld, CameraOffset>().WithAll<CameraFollow, GhostOwnerIsLocal>())
            {
                _currentCamera.transform.position = ltw.Position + cd.Value;
            }
        }
    }
}