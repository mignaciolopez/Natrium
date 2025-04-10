using CEG.Gameplay.Shared.Components;
using CEG.Gameplay.Shared.Components.Debug;
using CEG.Gameplay.Shared.Systems;
using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;

namespace CEG.Gameplay.Client.Systems.UI.Debug
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
            
            foreach (var (rpcAttack, rpcEntity) 
                     in SystemAPI.Query<RefRO<RPCAttack>>()
                         .WithAll<ReceiveRpcCommandRequest>()
                         .WithEntityAccess())
            {
                var entitySource = networkIdLookup.GetEntityPrefab(rpcAttack.ValueRO.NetworkIdSource);
                var entityTarget = networkIdLookup.GetEntityPrefab(rpcAttack.ValueRO.NetworkIdTarget);

                if (entitySource != Entity.Null && entityTarget != Entity.Null)
                {
                    Log.Debug(  $"Debugging Attack {entitySource}|{rpcAttack.ValueRO.NetworkIdSource} -> " +
                                $"{entityTarget}|{rpcAttack.ValueRO.NetworkIdTarget}" +
                                $"@ | {networkTime.ServerTick}");
                
                    //ToDo: Refactor | Find a way to avoid searching in children.
                    foreach (var child in SystemAPI.GetBuffer<LinkedEntityGroup>(entityTarget))
                    {
                        if (!state.EntityManager.HasComponent<DebugTag>(child.Value))
                            continue;

                        Log.Debug($"Enabling {child.Value}");
                        ecb.RemoveComponent<Disabled>(child.Value);
                        break;
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
