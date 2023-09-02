using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;
using Natrium.Gameplay.Components;
using Natrium.Shared;
using Natrium.Shared.Systems;

namespace Natrium.Gameplay.Systems
{

    [UpdateInGroup(typeof(GhostInputSystemGroup))]
    public partial class InputSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var pid in SystemAPI.Query<RefRW<PlayerInputData>>().WithAll<GhostOwnerIsLocal>())
            {
                pid.ValueRW = default;
                pid.ValueRW.InputAxis = new float3(Input.GetAxis("JHorizontal"), 0.0f, Input.GetAxis("JVertical"));

                if (pid.ValueRW.InputAxis is { x: 0, z: 0 })
                {
                    pid.ValueRW.InputAxis = new float3(Input.GetAxisRaw("Horizontal"), 0.0f, Input.GetAxisRaw("Vertical"));
                    pid.ValueRW.InputAxis = math.normalizesafe(pid.ValueRW.InputAxis);
                }

                if (Input.GetMouseButtonUp(0))
                    EventSystem.DispatchEvent(Events.OnPrimaryClick);
            }

            if (Input.GetKeyUp(KeyCode.Return))
                EventSystem.DispatchEvent(Events.ClientConnect);
            if (Input.GetKeyUp(KeyCode.Escape))
                EventSystem.DispatchEvent(Events.ClientDisconnect);

            ecb.Playback(EntityManager);
        }
    }
}