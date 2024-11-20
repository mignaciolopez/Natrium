using Natrium.Gameplay.Shared.Components;
using Natrium.Shared;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace Natrium.Gameplay.Shared.Systems.Initializers
{
    public partial struct InitializeDestroyOnTimerSystem : ISystem, ISystemStartStop
    {
        //[BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] | {this.ToString()}.OnCreate()");
            state.RequireForUpdate<NetworkTime>();
        }

        //[BurstCompile]
        public void OnStartRunning(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] | {this.ToString()}.OnStartRunning()");
        }

        //[BurstCompile]
        public void OnStopRunning(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] | {this.ToString()}.OnStopRunning()");
        }

        //[BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] | {this.ToString()}.OnDestroy()");
        }

        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var simulationTickRate = 15;//NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();
            var currentTick = networkTime.ServerTick;

            foreach (var (dot, e) in SystemAPI.Query<DestroyOnTimer>().WithNone<DestroyAtTick>().WithEntityAccess())
            {
                Log.Debug($"[{state.WorldUnmanaged.Name}] | Initializing DestroyOnTimer on: {e}");
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