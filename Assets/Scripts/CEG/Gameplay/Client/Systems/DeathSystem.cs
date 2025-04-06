using CEG.Gameplay.Shared.Components;
using Unity.Entities;
using Unity.NetCode;

namespace CEG.Gameplay.Client.Systems
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderLast = true)]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct DeathSystem : ISystem, ISystemStartStop
    {
        //[BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            Log.Verbose("OnCreate");
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

            foreach (var (speed, entity)
                     in SystemAPI.Query<RefRO<Speed>>()
                         .WithAll<DeathTag>()
                         .WithDisabled<ResurrectTag>()
                         .WithNone<DeathInitialized>()
                         .WithEntityAccess())
            {
                Log.Debug($"Killing {entity}");
                
                ecb.AddComponent<DeathInitialized>(entity);
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
    
}