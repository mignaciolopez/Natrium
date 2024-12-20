using Natrium.Gameplay.Shared.Components;
using Natrium.Gameplay.Shared.Components.Debug;
using Natrium.Gameplay.Shared.Systems;
using Natrium.Shared;
using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;

namespace Natrium.Gameplay.Client.Systems.UI.Debug
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct DebugAttackSystem : ISystem, ISystemStartStop
    {
        private BeginInitializationEntityCommandBufferSystem.Singleton _biEcbS;
        
        //[BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            Log.Verbose("OnCreate");
            state.RequireForUpdate<NetworkTime>();
            state.RequireForUpdate<NetworkIdLookup>();
            state.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
        }

        //[BurstCompile]
        public void OnStartRunning(ref SystemState state)
        {
            Log.Verbose("OnStartRunning");
            _biEcbS = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
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
            var ecb = _biEcbS.CreateCommandBuffer(state.WorldUnmanaged);
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();

            var networkIdLookup = SystemAPI.GetSingleton<NetworkIdLookup>();
            
            foreach (var (rpcAttack, receiveRpcCommandRequest, rpcEntity) 
                     in SystemAPI.Query<RefRO<RPCAttack>, RefRO<ReceiveRpcCommandRequest>>()
                         .WithEntityAccess())
            {
                var entitySource = networkIdLookup.GetEntityPrefab(rpcAttack.ValueRO.NetworkIdSource);
                var entityTarget = networkIdLookup.GetEntityPrefab(rpcAttack.ValueRO.NetworkIdTarget);

                if (entitySource != Entity.Null && entityTarget != Entity.Null)
                {
                    Log.Debug(  $"Debugging Attack {entitySource}|{rpcAttack.ValueRO.NetworkIdSource} -> " +
                                $"{entityTarget}|{rpcAttack.ValueRO.NetworkIdTarget}" +
                                $"@ | {networkTime.ServerTick}");
                
                    foreach (var child in SystemAPI.GetBuffer<LinkedEntityGroup>(entityTarget))
                    {
                        if (!state.EntityManager.HasComponent<DebugTag>(child.Value))
                            continue;
                        
                        Log.Debug($"Enabling {child.Value}");
                        ecb.RemoveComponent<Disabled>(child.Value);
                    }
                }
                else
                {
                    Log.Error($"Someone is Null: entitySource: {entitySource} | entityTarget: {entityTarget}");
                }
                
                //ToDo: Should not consume event just for debugging
                ecb.DestroyEntity(rpcEntity);
            }
        }
    }
}
