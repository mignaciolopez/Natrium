using CEG.Gameplay.Shared.Components;
using CEG.Shared;
using Unity.Entities;
using Unity.NetCode;

namespace CEG.Gameplay.Shared.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct DestroyOnTimerSystem : ISystem, ISystemStartStop
    {
        public void OnCreate(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] OnCreate");
            state.RequireForUpdate<NetworkTime>();
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
            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);

            var networkTime = SystemAPI.GetSingleton<NetworkTime>();

            foreach (var (destroyAtTick, entity) in SystemAPI.Query<RefRO<DestroyAtTick>>()
                         .WithNone<DestroyEntityTag>()
                         .WithEntityAccess())
            {
                if (!networkTime.ServerTick.IsValid ||
                    !destroyAtTick.ValueRO.Value.IsValid ||
                    networkTime.ServerTick.Equals(destroyAtTick.ValueRO.Value) || 
                    networkTime.ServerTick.IsNewerThan(destroyAtTick.ValueRO.Value))
                {
                    ecb.AddComponent<DestroyEntityTag>(entity);
                }
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}