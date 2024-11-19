using Natrium.Gameplay.Shared.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace Natrium.Gameplay.Shared.Systems
{
    public partial struct InitializeDestroyOnTimerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkTime>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var simulationTickRate = 60;// NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();
            var currentTick = networkTime.ServerTick;

            foreach (var (dot, e) in SystemAPI.Query<DestroyOnTimer>().WithNone<DestroyAtTick>().WithEntityAccess())
            {
                var lifeTimeInTicks = (uint)(dot.Value * simulationTickRate);
                var targetTick = currentTick;
                targetTick.Add(lifeTimeInTicks);
                ecb.AddComponent(e, new DestroyAtTick { Value = targetTick });
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}