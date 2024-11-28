using Natrium.Gameplay.Shared.Components;
using Natrium.Gameplay.Shared.Components.Debug;
using Natrium.Gameplay.Shared.Systems;
using Natrium.Shared;
//using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;

namespace Natrium.Gameplay.Client.Systems.UI.Debug
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderLast = true)]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct DebugAttackSystem : ISystem, ISystemStartStop
    {
        private NetworkTick _previousNetworkTick;
        
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
            _previousNetworkTick = SystemAPI.GetSingleton<NetworkTime>().InterpolationTick;
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
            ShowRedBoxForAttackedEntities(ref state);
        }

        //[BurstCompile]
        private void ShowRedBoxForAttackedEntities(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
            
            var networkIDLookup = SystemAPI.GetSingleton<NetworkIdLookup>();
            
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();
            var currentTick = networkTime.InterpolationTick;
            if (currentTick.TickIndexForValidTick == _previousNetworkTick.TickIndexForValidTick)
            {
                //Log.Debug($"currentTick {currentTick} is not newer than {_previousNetworkTick}. Skipping.");
                return;
            }
            _previousNetworkTick = currentTick;
            
            foreach (var attackEvents in SystemAPI.Query<DynamicBuffer<AttackEvents>>())
            {
                foreach (var attackEventServerVersion in attackEvents)
                {
                    if (currentTick.TickIndexForValidTick != attackEventServerVersion.Tick.TickIndexForValidTick)
                        continue;
                    
                    var attackEvent = attackEventServerVersion;
                    
                    //Updating Entities on client based on the networkIds sent from server.
                    attackEvent.EntitySource = networkIDLookup.GetEntityPrefab(attackEvent.NetworkIdSource);
                    attackEvent.EntityTarget = networkIDLookup.GetEntityPrefab(attackEvent.NetworkIdTarget);
                    
                    Log.Debug($"{attackEvent.EntitySource} is Attacking {attackEvent.EntityTarget} on Client Tick {currentTick}");
                    Log.Debug($"attackEvent.NetworkTick: {attackEvent.Tick}");
                    
                    foreach (var child in SystemAPI.GetBuffer<LinkedEntityGroup>(attackEvent.EntityTarget))
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
