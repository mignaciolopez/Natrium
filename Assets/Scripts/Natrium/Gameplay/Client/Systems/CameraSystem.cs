using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Natrium.Gameplay.Client.Components;
using Natrium.Shared;
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
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnCreate()");
            base.OnCreate();
        }

        protected override void OnStartRunning()
        {
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnStartRunning()");
            base.OnStartRunning();

            _currentCamera = Camera.main;
        }

        protected override void OnStopRunning()
        {
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnStopRunning()");
            base.OnStopRunning();
        }

        protected override void OnDestroy()
        {
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnDestroy()");
            base.OnDestroy();
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