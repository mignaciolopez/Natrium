using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;
using Natrium.Gameplay.Shared.Components;
using Natrium.Shared.Systems;

namespace Natrium.Gameplay.Client.Systems
{
    [UpdateInGroup(typeof(GhostInputSystemGroup))]
    public partial class InputSystem : SystemBase
    {
        protected override void OnCreate()
        {
            base.OnCreate();

            RequireForUpdate<InputSystemExecute>();
        }

        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var pid in SystemAPI.Query<RefRW<PlayerInput>>().WithAll<GhostOwnerIsLocal>())
            {
                pid.ValueRW = default;
                
                //Get Input From Joystick First
                pid.ValueRW.InputAxis = new float3(Input.GetAxis("JHorizontal"), 0.0f, Input.GetAxis("JVertical"));

                //If there was no Input, Get Input From Keyboard
                if (pid.ValueRW.InputAxis is { x: 0, z: 0 })
                {
                    pid.ValueRW.InputAxis = new float3(Input.GetAxisRaw("Horizontal"), 0.0f, Input.GetAxisRaw("Vertical"));
                    pid.ValueRW.InputAxis = math.normalizesafe(pid.ValueRW.InputAxis);
                }
                
                if (Input.GetMouseButtonUp(0))
                    EventSystem.EnqueueEvent(Natrium.Shared.Events.OnPrimaryClick);
            }

            if (Input.GetKeyUp(KeyCode.Return))
                EventSystem.EnqueueEvent(Natrium.Shared.Events.OnKeyCodeReturn);
            if (Input.GetKeyUp(KeyCode.Escape))
                EventSystem.EnqueueEvent(Natrium.Shared.Events.OnKeyCodeEscape);
            if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.RightControl))
                throw new NotImplementedException("Input KeyCode.LeftControl || KeyCode.RightControl");

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}