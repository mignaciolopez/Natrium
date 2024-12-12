using Natrium.Gameplay.Shared.Components;
using Natrium.Gameplay.Shared.Components.Debug;
using Natrium.Gameplay.Shared.Systems;
using Natrium.Shared;
//using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;

namespace Natrium.Gameplay.Client.Systems.UI.Debug
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct DebugAttackSystem : ISystem, ISystemStartStop
    {
        //[BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            Log.Verbose("OnCreate");
            state.RequireForUpdate<NetworkTime>();
            state.RequireForUpdate<NetworkIdLookup>();
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

            if (!networkTime.IsFirstTimeFullyPredictingTick)
                return;

            var networkIdLookup = SystemAPI.GetSingleton<NetworkIdLookup>();
            
            foreach (var (attacksBuffer, entity)
                     in SystemAPI.Query<DynamicBuffer<AttacksBuffer>>()
                         .WithAll<Simulate>()
                         .WithEntityAccess())
            {
                //Log.Debug($"Querying {nameof(AttacksBuffer)}");
                
                foreach (var attack in attacksBuffer)
                {
                    if (networkTime.InterpolationTick.IsNewerThan(attack.ServerTick))
                        continue;
                    
                    var entitySource = networkIdLookup.GetEntityPrefab(attack.NetworkIdSource);
                    
                    Log.Debug($"Debugging Attack {entitySource} -> {entity}@i{attack.InterpolationTick}|i{networkTime.InterpolationTick}");
                    
                    foreach (var child in SystemAPI.GetBuffer<LinkedEntityGroup>(entity))
                    {
                        if (!state.EntityManager.HasComponent<DebugTag>(child.Value))
                            continue;
                    
                        Log.Debug($"Enabling {child.Value}");
                        ecb.RemoveComponent<Disabled>(child.Value);
                    }
                }
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
