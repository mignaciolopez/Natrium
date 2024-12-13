using Natrium.Gameplay.Shared.Components;
using Natrium.Shared;
using Unity.Entities;
using Unity.NetCode;
using Unity.NetCode.LowLevel.Unsafe;

namespace Natrium.Gameplay.Server.Systems
{
    [UpdateInGroup(typeof(GhostSimulationSystemGroup))]
    [UpdateAfter(typeof(AimSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct AttackSystem : ISystem, ISystemStartStop
    {
        public void OnCreate(ref SystemState state)
        {
            Log.Verbose("OnCreate");
            state.RequireForUpdate<NetworkTime>();
        }

        public void OnStartRunning(ref SystemState state)
        {
            Log.Verbose("OnStartRunning");
        }

        public void OnStopRunning(ref SystemState state)
        {
            Log.Verbose("OnStopRunning");
        }

        public void OnDestroy(ref SystemState state)
        {
            Log.Verbose("OnDestroy");
        }
        
        public void OnUpdate(ref SystemState state)
        {
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();
            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
            
            foreach (var (attacksBuffer, ghostOwner, entity) in SystemAPI.Query<DynamicBuffer<AttacksBuffer>, RefRO<GhostOwner>>()
                         .WithDisabled<DeathTag>()
                         .WithEntityAccess())
            {
                foreach (var attack in attacksBuffer)
                {
                    if (attack.EntitySource == entity)
                    {
                        Log.Warning($"{entity} is attacking itself.");
                        //continue;
                    }
                    
                    var rpcEntity = ecb.CreateEntity();
                    ecb.AddComponent(rpcEntity, new RPCAttack
                    {
                        NetworkIdSource = state.EntityManager.GetComponentData<GhostOwner>(attack.EntitySource).NetworkId,
                        NetworkIdTarget = ghostOwner.ValueRO.NetworkId,
                    });
                    ecb.AddComponent<SendRpcCommandRequest>(rpcEntity);
                    
                    //ToDo: Should not do damage at this moment
                    Log.Debug($"{attack.EntitySource} is Dealing Damage on {entity}@|{networkTime.ServerTick}");
                    
                    var damagePoints = SystemAPI.GetComponentRO<DamagePoints>(attack.EntitySource);
                    var damageBuffer = state.EntityManager.GetBuffer<DamagePointsBuffer>(entity);
                    damageBuffer.Add(new DamagePointsBuffer { Value = damagePoints.ValueRO.Value });
                }
                
                attacksBuffer.Clear();
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        
    }
}
