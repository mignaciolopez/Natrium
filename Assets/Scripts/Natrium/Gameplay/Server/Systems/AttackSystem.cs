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
            
            foreach (var (attacksBuffer, entity) in SystemAPI.Query<DynamicBuffer<AttacksBuffer>>()
                         .WithDisabled<DeathTag>()
                         .WithEntityAccess())
            {
                foreach (var attack in attacksBuffer)
                {
                    if (networkTime.ServerTick.IsNewerThan(attack.ServerTick))
                        continue;
                    
                    if (attack.EntitySource == entity)
                    {
                        Log.Warning($"{entity} is attacking itself.");
                        //continue;
                    }
                    
                    //ToDo: Should not do damage at this moment
                    Log.Debug($"{attack.EntitySource} is Dealing Damage on {entity}@{attack.ServerTick}|{networkTime.ServerTick}");
                    
                    var damagePoints = SystemAPI.GetComponentRO<DamagePoints>(attack.EntitySource);
                    var damageBuffer = state.EntityManager.GetBuffer<DamagePointsBuffer>(entity);
                    damageBuffer.Add(new DamagePointsBuffer { Value = damagePoints.ValueRO.Value });
                }
            }
        }

        
    }
}
