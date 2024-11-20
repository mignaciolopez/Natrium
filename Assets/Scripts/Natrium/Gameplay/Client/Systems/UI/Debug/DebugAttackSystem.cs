using Natrium.Gameplay.Shared.Components;
using Natrium.Gameplay.Shared.Components.Debug;
using Natrium.Gameplay.Shared.Utilities;
using Natrium.Shared;
//using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;

namespace Natrium.Gameplay.Client.Systems.UI.Debug
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderLast = true)]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct DebugAttackSystem : ISystem, ISystemStartStop
    {
        //[BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] | {this.ToString()}.OnCreate()");
            state.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
        }

        //[BurstCompile]
        public void OnStartRunning(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] | {this.ToString()}.OnStartRunning()");
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
        
        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            QueryAndShowInputAims(ref state);
        }

        //[BurstCompile]
        private void QueryAndShowInputAims(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
            
            foreach (var (rpcAttack, rpcEntity)
                     in SystemAPI.Query<RefRO<RPCAttack>>().WithAll<ReceiveRpcCommandRequest>().WithEntityAccess())
            {
                Log.Debug($"[{state.WorldUnmanaged.Name}] | Processing RPCAttack");
                var entityTarget = Utils.GetEntityPrefab(rpcAttack.ValueRO.NetworkIdTarget, state.EntityManager);
                foreach (var child in SystemAPI.GetBuffer<LinkedEntityGroup>(entityTarget))
                {
                    if (!state.EntityManager.HasComponent<DebugTag>(child.Value))
                        continue;
                    
                    Log.Debug($"[{state.WorldUnmanaged.Name}] | Enabling {child.Value}");
                    ecb.RemoveComponent<Disabled>(child.Value);
                }
                
                ecb.DestroyEntity(rpcEntity);
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
