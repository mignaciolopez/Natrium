using Natrium.Gameplay.Shared.Components;
using Natrium.Shared;
using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;

namespace Natrium.Gameplay.Client.Systems
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderLast = true)]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct DisableEntitySystem : ISystem, ISystemStartStop
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
            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();
            var currentTick = state.WorldUnmanaged.IsServer() ? networkTime.ServerTick : networkTime.InterpolationTick;
            
            foreach (var (disableAtTick, e) in SystemAPI.Query<RefRO<DisableAtTick>>()
                         .WithNone<GhostOwner, Disabled>().WithEntityAccess())//Excluding GhostOwners, Client should never disable authoritative data from the server.
            {
                if (!currentTick.IsNewerThan(disableAtTick.ValueRO.Value))
                    continue;

                Log.Debug($"currentTick: {currentTick}");
                Log.Debug($"Disabling {e} on tick: {disableAtTick.ValueRO.Value}");
                ecb.AddComponent<Disabled>(e);
                ecb.RemoveComponent<DisableAtTick>(e);
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}