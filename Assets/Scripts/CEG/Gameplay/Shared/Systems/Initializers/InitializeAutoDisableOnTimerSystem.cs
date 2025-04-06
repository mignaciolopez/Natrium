using CEG.Gameplay.Shared.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace CEG.Gameplay.Shared.Systems.Initializers
{
    public partial struct InitializeAutoDisableOnTimerSystem : ISystem, ISystemStartStop
    {
        private int _simulationTickRate;
        
        //[BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] OnCreate");
            state.RequireForUpdate<NetworkTime>();
        }

        //[BurstCompile]
        public void OnStartRunning(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] OnStartRunning");
            _simulationTickRate = NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;
        }

        //[BurstCompile]
        public void OnStopRunning(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] OnStopRunning");
        }

        //[BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] OnDestroy");
        }

        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();
            var currentTick = state.WorldUnmanaged.IsServer() ? networkTime.ServerTick : networkTime.InterpolationTick;

            foreach (var (dot, e) in SystemAPI.Query<RefRO<DisableOnTimer>>().WithNone<DisableAtTick>().WithEntityAccess())
            {
                Log.Debug($"[{state.WorldUnmanaged.Name}] | Initializing {e} DisableOnTimer on tick: {currentTick}");
                var lifeTimeInTicks = (uint)(dot.ValueRO.Value * _simulationTickRate);
                var targetTick = currentTick;
                targetTick.Add(lifeTimeInTicks);
                ecb.AddComponent(e, new DisableAtTick { Value = targetTick });
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}