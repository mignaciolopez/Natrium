using Natrium.Gameplay.Shared.Components;
using Natrium.Shared;
using Unity.Entities;
using Unity.NetCode;

namespace Natrium.Gameplay.Server.Systems
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [UpdateAfter(typeof(AimSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class AttackSystem : SystemBase
    {
        protected override void OnCreate()
        {
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnCreate()");
            base.OnCreate();
        }

        protected override void OnStartRunning()
        {
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnStartRunning()");
            base.OnStartRunning();
        }

        protected override void OnStopRunning()
        {
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnStopRunning()");
            base.OnStopRunning();
        }

        protected override void OnDestroy()
        {
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnDestroy()");
            base.OnDestroy();
        }
        
        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(WorldUpdateAllocator);
            
            foreach (var (attack, ghostOwner, e) 
                     in SystemAPI.Query<RefRO<Attack>, RefRO<GhostOwner>>()
                         .WithAll<AttackableTag>()
                         .WithDisabled<DeathTag>()
                         .WithEntityAccess())
            {
                ecb.SetComponentEnabled<Attack>(e, false);
                if (attack.ValueRO.SourceServerEntity == e)
                {
                    Log.Warning($"{e} is attacking itself.");
                    //continue;
                }
                Log.Debug($"{attack.ValueRO.SourceServerEntity} is Dealing Damage on {e}");

                var damagePoints = SystemAPI.GetComponentRO<DamagePoints>(attack.ValueRO.SourceServerEntity);
                var goSource = SystemAPI.GetComponentRO<GhostOwner>(attack.ValueRO.SourceServerEntity);
                var damageBuffer = EntityManager.GetBuffer<DamagePointsBuffer>(e);
                damageBuffer.Add(new DamagePointsBuffer { Value = damagePoints.ValueRO.Value });
                
                var req = ecb.CreateEntity();
                ecb.AddComponent(req, new RPCAttack
                {
                    NetworkIdSource = goSource.ValueRO.NetworkId,
                    NetworkIdTarget = ghostOwner.ValueRO.NetworkId,
                });
                ecb.AddComponent<SendRpcCommandRequest>(req); //Broadcast
            }
            
            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}
