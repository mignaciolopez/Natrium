using Natrium.Gameplay.Shared.Components;
using Unity.Entities;
using Unity.NetCode;

namespace Natrium.Gameplay.Server.Systems
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    public partial struct DestroyOnTimerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkTime>();
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecbs = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbs.CreateCommandBuffer(state.WorldUnmanaged);

            var currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
            if (currentTick.IsValid)
            {
                foreach (var (dat, e) in SystemAPI.Query<DestroyAtTick>().WithAll<Simulate>()
                    .WithNone<DestroyEntityTag>().WithEntityAccess())
                {
                    if (dat.Value.IsValid)
                    {
                        if (currentTick.Equals(dat.Value) || currentTick.IsNewerThan(dat.Value))
                        {
                            ecb.AddComponent<DestroyEntityTag>(e);
                        }
                    }
                }
            }
        }
    }
}