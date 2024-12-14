using Natrium.Gameplay.Shared.Components;
using Natrium.Shared;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;

namespace Natrium.Gameplay.Client.Systems
{
    [UpdateInGroup(typeof(MovementSystemGroup))]
    [UpdateBefore(typeof(MovementSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct ReckoningSystem : ISystem, ISystemStartStop
    {
        //[BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            Log.Verbose("OnCreate");
            state.RequireForUpdate<NetworkTime>();
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
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();
            
            if (!networkTime.IsFirstTimeFullyPredictingTick)
                return;
            
            foreach (var (localTransform, movementData, reckoning) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<MovementData>, RefRO<Reckoning>>()
                         .WithAll<PredictedGhost, Simulate>())
            {
                //if (reckoning.ValueRO.Tick == networkTime.ServerTick)
                if (reckoning.ValueRO.ShouldReckon)
                {
                    Log.Debug($"Reckoning Tick {reckoning.ValueRO.Tick}@{networkTime.ServerTick}");
                    localTransform.ValueRW.Position = reckoning.ValueRO.Target;
                    movementData.ValueRW.Target = reckoning.ValueRO.Target;
                    movementData.ValueRW.Previous = reckoning.ValueRO.Target;
                }
            }
        }
    }
}