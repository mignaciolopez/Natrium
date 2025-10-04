using CEG.Logging;
using Gameplay.Shared.Components;
using Unity.Entities;
using Unity.NetCode;

namespace Gameplay.Shared.Systems.Initializers
{
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    public partial struct InitializeDestroyOnTimerSystem : ISystem, ISystemStartStop
    {
        //[BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            Log.Verbose($"OnCreate");
            state.RequireForUpdate<NetworkTime>();
        }

        //[BurstCompile]
        public void OnStartRunning(ref SystemState state)
        {
            Log.Verbose($"OnStartRunning");
        }

        //[BurstCompile]
        public void OnStopRunning(ref SystemState state)
        {
            Log.Verbose($"OnStopRunning");
        }

        //[BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            Log.Verbose($"OnDestroy");
        }

        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
            var simulationTickRate = NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();

            foreach (var (destroyOnTimer, entity)
                     in SystemAPI.Query<RefRO<DestroyOnTimer>>()
                         .WithNone<DestroyAtTick>()
                         .WithEntityAccess())
            {
                Log.Debug($"Initializing {nameof(DestroyOnTimer)} on: {entity}");
                var lifeTimeInTicks = (uint)(destroyOnTimer.ValueRO.Value * simulationTickRate);
                var targetTick = networkTime.ServerTick;
                targetTick.Add(lifeTimeInTicks);
                ecb.AddComponent(entity, new DestroyAtTick { Value = targetTick });
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}