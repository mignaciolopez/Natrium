using Unity.Collections;
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
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var pid in SystemAPI.Query<RefRW<PlayerInputData>>().WithAll<GhostOwnerIsLocal>())
            {
                pid.ValueRW = default;
                pid.ValueRW.InputAxis = new float3(Input.GetAxis("JHorizontal"), 0.0f, Input.GetAxis("JVertical"));

                if (pid.ValueRW.InputAxis.x == 0 && pid.ValueRW.InputAxis.z == 0)
                {
                    pid.ValueRW.InputAxis = new float3(Input.GetAxisRaw("Horizontal"), 0.0f, Input.GetAxisRaw("Vertical"));
                    pid.ValueRW.InputAxis = math.normalizesafe(pid.ValueRW.InputAxis);
                }

                if (Input.GetMouseButtonUp(0))
                    EventSystem.DispatchEvent(Events.OnPrimaryClick);
            }

            if (Input.GetKeyUp(KeyCode.Return))
                EventSystem.DispatchEvent(Events.Client_Connect);
            if (Input.GetKeyUp(KeyCode.Escape))
                EventSystem.DispatchEvent(Events.Client_Disconnect);

            ecb.Playback(EntityManager);
        }
    }
}