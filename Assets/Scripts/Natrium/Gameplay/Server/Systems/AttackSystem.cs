using Natrium.Gameplay.Shared.Components;
using Natrium.Shared;
using Unity.Entities;
using Unity.NetCode;
using Unity.NetCode.LowLevel.Unsafe;

namespace Natrium.Gameplay.Server.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(AimSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class AttackSystem : SystemBase
    {
        protected override void OnCreate()
        {
            Log.Verbose("OnCreate");
            base.OnCreate();
        }

        protected override void OnStartRunning()
        {
            Log.Verbose("OnStartRunning");
            base.OnStartRunning();
        }

        protected override void OnStopRunning()
        {
            Log.Verbose("OnStopRunning");
            base.OnStopRunning();
        }

        protected override void OnDestroy()
        {
            Log.Verbose("OnDestroy");
            base.OnDestroy();
        }
        
        protected override void OnUpdate()
        {
            var currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
            
            foreach (var attackEvents in SystemAPI.Query<DynamicBuffer<AttackEvents>>()
                         .WithAll<AttackableTag>()
                         .WithDisabled<DeathTag>())
            {
                foreach (var attackEvent in attackEvents)
                {
                    if (currentTick.IsNewerThan(attackEvent.Tick))
                        continue;
                    
                    if (attackEvent.EntitySource == attackEvent.EntityTarget)
                    {
                        Log.Warning($"{attackEvent.EntitySource} is attacking itself.");
                        //continue;
                    }
                    
                    //ToDo: Should not do damage at this moment
                    Log.Debug($"{attackEvent.EntitySource} is Dealing Damage on {attackEvent.EntityTarget} on Server Tick {currentTick}");
                    Log.Debug($"attackEvent.NetworkTick: {attackEvent.Tick}");
                    
                    var damagePoints = SystemAPI.GetComponentRO<DamagePoints>(attackEvent.EntitySource);
                    var damageBuffer = EntityManager.GetBuffer<DamagePointsBuffer>(attackEvent.EntityTarget);
                    damageBuffer.Add(new DamagePointsBuffer { Value = damagePoints.ValueRO.Value });
                }
            }
        }
    }
}
