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
        private BeginSimulationEntityCommandBufferSystem.Singleton _bsEcbS;
        
        //[BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] | {this.ToString()}.OnCreate()");
            state.RequireForUpdate<NetworkTime>();
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }

        //[BurstCompile]
        public void OnStartRunning(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] | {this.ToString()}.OnStartRunning()");
            _bsEcbS = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
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

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();
            if (!networkTime.IsFirstTimeFullyPredictingTick)
                return;
            
            var ecb = _bsEcbS.CreateCommandBuffer(state.WorldUnmanaged);
            new DisableEntityJob { ECB = ecb }.Schedule();
        }
    }

    //[BurstCompile]
    [WithAll(typeof(DisableAtTick))]
    [WithNone( typeof(GhostOwner), typeof(Disabled))] //Excluding GhostOwners, Client should never disable authoritative data from the server.
    public partial struct DisableEntityJob : IJobEntity
    {
        public EntityCommandBuffer ECB;
        private void Execute(Entity e)
        {
            //Log.Debug($"[{this}] | Disabling {e}");
            ECB.AddComponent<Disabled>(e);
            ECB.RemoveComponent<DisableAtTick>(e);
        }
    }
}