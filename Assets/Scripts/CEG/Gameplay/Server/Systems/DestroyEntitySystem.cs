using CEG.Gameplay.Shared.Components;
using Unity.Burst;
using Unity.Entities;

namespace CEG.Gameplay.Server.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct DestroyEntitySystem : ISystem, ISystemStartStop
    {
        private BeginSimulationEntityCommandBufferSystem.Singleton _bsEcbS;
        
        //[BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            Log.Verbose("OnCreate");
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
            var ecb = _bsEcbS.CreateCommandBuffer(state.WorldUnmanaged);
            //new DestroyEntitySystemJob { ECB = ecb }.Schedule();
            
            foreach (var (destroyEntityTag, entity) in SystemAPI.Query<RefRO<DestroyEntityTag>>() 
                         .WithEntityAccess()) //Server Is authoritative and can destroy a GhostOwner
            {
                Log.Debug($"Destroying {entity}");
                ecb.DestroyEntity(entity);
            }
        }
    }

    /*[BurstCompile]
    [WithAll(typeof(DestroyEntityTag))] //Server Is authoritative and can destroy a GhostOwner
    public partial struct DestroyEntitySystemJob : IJobEntity
    {
        public EntityCommandBuffer ECB;
        private void Execute(Entity e)
        {
            ECB.DestroyEntity(e);
        }
    }*/
}