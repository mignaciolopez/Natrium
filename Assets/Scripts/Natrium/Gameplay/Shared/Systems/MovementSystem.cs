using Natrium.Gameplay.Shared.Components;
using Natrium.Shared;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

namespace Natrium.Gameplay.Shared.Systems
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial struct MovementSystem : ISystem, ISystemStartStop
    {
        private ComponentLookup<MovementFree> _lookUpFree;
        private ComponentLookup<MovementDiagonal> _lookUpDiagonal;
        private ComponentLookup<MovementClassic> _lookUpClassic;

        //[BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] OnCreate");
            _lookUpFree = state.GetComponentLookup<MovementFree>();
            _lookUpDiagonal = state.GetComponentLookup<MovementDiagonal>();
            _lookUpClassic = state.GetComponentLookup<MovementClassic>();
        }

        public void OnStartRunning(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] OnStartRunning");
        }
        
        public void OnStopRunning(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] OnStopRunning");
        }
        
        public void OnDestroy(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] OnDestroy");
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var dt = SystemAPI.Time.DeltaTime;

            _lookUpFree.Update(ref state);
            _lookUpDiagonal.Update(ref state);
            _lookUpClassic.Update(ref state);

            state.Dependency = new UpdateMovementTypeJob
            {
                LookUpFree = _lookUpFree,
                LookUpDiagonal = _lookUpDiagonal,
                LookUpClassic = _lookUpClassic
            }.Schedule(state.Dependency);

            state.Dependency = new MoveTowardsJob
            {
                DeltaTime = dt
            }.Schedule(state.Dependency);
        }
    } //MovementSystem

    [BurstCompile]
    public partial struct UpdateMovementTypeJob : IJobEntity
    {
        public ComponentLookup<MovementFree> LookUpFree;
        public ComponentLookup<MovementDiagonal> LookUpDiagonal;
        public ComponentLookup<MovementClassic> LookUpClassic;

        private void Execute(in MovementType mt, Entity e)
        {
            switch (mt.Value)
            {
                case MovementTypeEnum.Free:
                    LookUpFree.SetComponentEnabled(e, true);
                    LookUpDiagonal.SetComponentEnabled(e, false);
                    LookUpClassic.SetComponentEnabled(e, false);
                    break;
                case MovementTypeEnum.Diagonal:
                    LookUpFree.SetComponentEnabled(e, false);
                    LookUpDiagonal.SetComponentEnabled(e, true);
                    LookUpClassic.SetComponentEnabled(e, false);
                    break;
                case MovementTypeEnum.Classic:
                    LookUpFree.SetComponentEnabled(e, false);
                    LookUpDiagonal.SetComponentEnabled(e, false);
                    LookUpClassic.SetComponentEnabled(e, true);
                    break;
            }
        }
    }

    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct MoveTowardsJob : IJobEntity
    {
        public float DeltaTime;

        private static float3 MoveTowards(float3 current, float3 target, float maxDistanceDelta)
        {
            var num = target.x - current.x;
            var num2 = target.y - current.y;
            var num3 = target.z - current.z;
            var num4 = num * num + num2 * num2 + num3 * num3;
            if (num4 == 0f || (maxDistanceDelta >= 0f && num4 <= maxDistanceDelta * maxDistanceDelta))
            {
                return target;
            }

            var num5 = math.sqrt(num4);
            return new float3(current.x + num / num5 * maxDistanceDelta, current.y + num2 / num5 * maxDistanceDelta, current.z + num3 / num5 * maxDistanceDelta);
        }

        private void Execute(ref LocalTransform lt, in PlayerTilePosition ptp, in Speed speed)
        {
            lt.Position = MoveTowards(lt.Position, ptp.Target, speed.Value * DeltaTime);
        }
    }
} // namespace