using CEG.Gameplay.Shared.Components;
using CEG.Shared;
using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;

namespace CEG.Gameplay.Client.Systems
{
    [UpdateInGroup(typeof(GhostSimulationSystemGroup), OrderLast = true)]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct DisableEntitySystem : ISystem, ISystemStartStop
    {
        //[BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            Log.Verbose("OnCreate");
            state.RequireForUpdate<NetworkTime>();
        }

        //[BurstCompile]
        public void OnStartRunning(ref SystemState state)
        {
            Log.Verbose("OnStartRunning");
        }

        //[BurstCompile]
        public void OnStopRunning(ref SystemState state)
        {
            Log.Verbose("OnStopRunning");
        }

        //[BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            Log.Verbose("OnDestroy");
        }

        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();
            
            foreach (var (disableAtTick, e) in SystemAPI.Query<RefRO<DisableAtTick>>()
                         .WithNone<Disabled>().WithEntityAccess()) //GhostOwner used to be excluded here
            {
                if (networkTime.ServerTick.Equals(disableAtTick.ValueRO.Value) ||
                    networkTime.ServerTick.IsNewerThan(disableAtTick.ValueRO.Value))
                {
                    Log.Debug($"Disabling {e}@{disableAtTick.ValueRO.Value}|{networkTime.ServerTick}");
                    ecb.AddComponent<Disabled>(e);
                    ecb.RemoveComponent<DisableAtTick>(e);
                }
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}