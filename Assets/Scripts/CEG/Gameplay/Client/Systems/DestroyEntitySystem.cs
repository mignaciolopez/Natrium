using CEG.Gameplay.Shared.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;

namespace CEG.Gameplay.Client.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct DestroyEntitySystem : ISystem, ISystemStartStop
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
            //new DestroyEntityJob { ECB = ecb }.Schedule();

            foreach (var (destroyEntityTag, entity) in SystemAPI.Query<RefRO<DestroyEntityTag>>() 
                         .WithNone<GhostOwner>() //Excluding GhostOwners, Client should Never Destroy authoritative data from the server
                         .WithEntityAccess())
            {
                Log.Debug($"Destroying {entity}");
                ecb.DestroyEntity(entity);
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
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