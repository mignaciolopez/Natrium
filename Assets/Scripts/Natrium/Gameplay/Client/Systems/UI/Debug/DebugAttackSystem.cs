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
        //[BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            Log.Verbose("OnCreate");
            state.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
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
            QueryAndShowInputAims(ref state);
        }

        //[BurstCompile]
        private void QueryAndShowInputAims(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);

            var networkIDLookup = SystemAPI.GetSingleton<NetworkIdLookup>();
            
            foreach (var (rpcAttack, rpcEntity)
                     in SystemAPI.Query<RefRO<RPCAttack>>().WithAll<ReceiveRpcCommandRequest>().WithEntityAccess())
            {
                Log.Debug($"Processing rpcAttack {rpcAttack}");
                var entityTarget = networkIDLookup.GetEntityPrefab(rpcAttack.ValueRO.NetworkIdTarget);
                if (entityTarget == Entity.Null)
                {
                    Log.Warning($"networkIDLookup returned null for NetworkId {rpcAttack.ValueRO.NetworkIdTarget}");
                    return;
                }
                
                foreach (var child in SystemAPI.GetBuffer<LinkedEntityGroup>(entityTarget))
                {
                    if (!state.EntityManager.HasComponent<DebugTag>(child.Value))
                        continue;
                    
                    Log.Debug($"Enabling {child.Value}");
                    ecb.RemoveComponent<Disabled>(child.Value);
                }
                
                ecb.DestroyEntity(rpcEntity);
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
