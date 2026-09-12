using CEG.Gameplay.Shared.Components;
using CyberEntt.Logging;
using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using static Unity.Entities.SystemAPI;

namespace CEG.Gameplay.Shared.Systems.Initializers
{
    public partial struct InitializeAutoDisableOnTimerSystem : ISystem, ISystemStartStop
    {
        private int _simulationTickRate;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            Log.Verbose("OnCreate", ref state);
            state.RequireForUpdate<NetworkTime>();
        }

        //[BurstCompile]
        public void OnStartRunning(ref SystemState state)
        {
            Log.Verbose("OnStartRunning", ref state);
            _simulationTickRate = NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;
        }

        [BurstCompile]
        public void OnStopRunning(ref SystemState state)
        {
            Log.Verbose("OnStopRunning", ref state);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            Log.Verbose("OnDestroy", ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
            
            var networkTime = GetSingleton<NetworkTime>();
            var currentTick = state.WorldUnmanaged.IsServer() ? networkTime.ServerTick : networkTime.InterpolationTick;

            foreach (var (dot, e) in Query<RefRO<DisableOnTimer>>().WithNone<DisableAtTick>().WithEntityAccess())
            {
                Log.Debug($"Initializing DisableOnTimer on Entity:'{e.Index}' on tick:'{currentTick}'", ref state);
                var lifeTimeInTicks = (uint)(dot.ValueRO.Value * _simulationTickRate);
                var targetTick = currentTick;
                targetTick.Add(lifeTimeInTicks);
                ecb.AddComponent(e, new DisableAtTick { Value = targetTick });
            }

            ecb.Playback(state.EntityManager);
        }
    }
}