using Natrium.Gameplay.Shared.Components;
using Natrium.Shared;
using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;

namespace Natrium.Gameplay.Client.Systems
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderLast = true)]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct DestroyEntitySystem : ISystem, ISystemStartStop
    {
        private BeginSimulationEntityCommandBufferSystem.Singleton _bsEcbS;
        
        //[BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            Log.Verbose("OnCreate");
            state.RequireForUpdate<NetworkTime>();
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }

        //[BurstCompile]
        public void OnStartRunning(ref SystemState state)
        {
            Log.Verbose("OnStartRunning");
            _bsEcbS = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
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
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();
            if (!networkTime.IsFirstTimeFullyPredictingTick)
                return;
            
            var ecb = _bsEcbS.CreateCommandBuffer(state.WorldUnmanaged);
            //new DestroyEntityJob { ECB = ecb }.Schedule();

            foreach (var (localTransform, e) in SystemAPI.Query<RefRO<LocalTransform>>() 
                         .WithAll<DestroyEntityTag>()
                         .WithNone<GhostOwner>() //Excluding GhostOwners, Client should Never Destroy authoritative data from the server
                         .WithEntityAccess())
            {
                Log.Debug($"Destroying {e}");
                ecb.DestroyEntity(e);
            }
        }
    }

    /*[BurstCompile]
    [WithAll(typeof(DestroyEntityTag))]
    [WithNone( typeof(GhostOwner))] //Excluding GhostOwners, Client should Never Destroy authoritative data from the server
    public partial struct DestroyEntityJob : IJobEntity
    {
        public EntityCommandBuffer ECB;
        private void Execute(Entity e)
        {
            ECB.DestroyEntity(e);
        }
    }*/
}