using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using Unity.Collections;
using Natrium.Shared.Systems;
using Natrium.Gameplay.Shared.Components;

namespace Natrium.Client.Gameplay.Systems
{    
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class AimSystem : SystemBase
    {
        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            EventSystem.Subscribe(Shared.Events.OnPrimaryClick, OnPrimaryClick);
        }

        protected override void OnStopRunning()
        {
            base.OnStopRunning();
            EventSystem.UnSubscribe(Shared.Events.OnPrimaryClick, OnPrimaryClick);
        }

        protected override void OnUpdate()
        {
            
        }

        private void OnPrimaryClick(Shared.Stream stream)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            if (Camera.main != null)
            {
                var mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition + new Vector3(0, 0, Camera.main.transform.position.y));

                Debug.Log($"{World.Unmanaged.Name} Input.mousePosition {Input.mousePosition}");
                Debug.Log($"{World.Unmanaged.Name} mouseWorldPosition {mouseWorldPosition}");

                var req = ecb.CreateEntity();
                ecb.AddComponent(req, new RpcAim { MouseWorldPosition = mouseWorldPosition });
                ecb.AddComponent<SendRpcCommandRequest>(req);
            }

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}