using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

namespace Natrium
{

    [UpdateInGroup(typeof(GhostInputSystemGroup))]
    public partial class InputSystem : SystemBase
    {
        protected override void OnCreate()
        {
            base.OnCreate();
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
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
            foreach (var lapd in SystemAPI.Query<RefRW<LocalActivePlayerData>>().WithAll<GhostOwnerIsLocal>())
            {
                lapd.ValueRW.InputAxis = new float3(Input.GetAxis("JHorizontal"), 0.0f, Input.GetAxis("JVertical"));
                if (lapd.ValueRW.InputAxis.x == 0 && lapd.ValueRW.InputAxis.z == 0)
                    lapd.ValueRW.InputAxis = new float3(Input.GetAxisRaw("Horizontal"), 0.0f, Input.GetAxisRaw("Vertical"));
            }
        }
    }
}