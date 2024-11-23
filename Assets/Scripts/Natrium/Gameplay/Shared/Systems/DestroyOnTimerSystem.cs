using Natrium.Gameplay.Shared.Components;
using Natrium.Shared;
using Unity.Entities;
using Unity.NetCode;

namespace Natrium.Gameplay.Shared.Systems
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    public partial struct DestroyOnTimerSystem : ISystem, ISystemStartStop
    {
        public void OnCreate(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] OnCreate");
            state.RequireForUpdate<NetworkTime>();
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }
        
        public void OnStartRunning(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] OnStartRunning");
        }

        public void OnStopRunning(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] OnStopRunning");
        }

        public void OnDestroy(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] OnDestroy");
        }

        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            var currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;

            foreach (var (destroyAtTick, entity) in SystemAPI.Query<DestroyAtTick>().WithAll<Simulate>()
                         .WithNone<DestroyEntityTag>().WithEntityAccess())
            {
                if (currentTick.Equals(destroyAtTick.Value) || currentTick.IsNewerThan(destroyAtTick.Value))
                {
                    ecb.AddComponent<DestroyEntityTag>(entity);
                }
            }
        }
    }
}