using Natrium.Gameplay.Shared.Components;
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

        private BeginInitializationEntityCommandBufferSystem.Singleton _ecbs;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _lookUpFree = state.GetComponentLookup<MovementFree>();
            _lookUpDiagonal = state.GetComponentLookup<MovementDiagonal>();
            _lookUpClassic = state.GetComponentLookup<MovementClassic>();
        }

        [BurstCompile]
        public void OnStartRunning(ref SystemState state)
        {
            _ecbs = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnStopRunning(ref SystemState state)
        {
            
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
                lookUpFree = _lookUpFree,
                lookUpDiagonal = _lookUpDiagonal,
                lookUpClassic = _lookUpClassic
            }.Schedule(state.Dependency);

            state.Dependency = new MoveTowardsJob
            {
                dt = dt
            }.Schedule(state.Dependency);
        }
    } //MovementSystem

    [BurstCompile]
    public partial struct UpdateMovementTypeJob : IJobEntity
    {
        public ComponentLookup<MovementFree> lookUpFree;
        public ComponentLookup<MovementDiagonal> lookUpDiagonal;
        public ComponentLookup<MovementClassic> lookUpClassic;

        private void Execute(in MovementType mt, Entity e)
        {
            switch (mt.Value)
            {
                case MovementTypeEnum.Free:
                    lookUpFree.SetComponentEnabled(e, true);
                    lookUpDiagonal.SetComponentEnabled(e, false);
                    lookUpClassic.SetComponentEnabled(e, false);
                    break;
                case MovementTypeEnum.Diagonal:
                    lookUpFree.SetComponentEnabled(e, false);
                    lookUpDiagonal.SetComponentEnabled(e, true);
                    lookUpClassic.SetComponentEnabled(e, false);
                    break;
                case MovementTypeEnum.Classic:
                    lookUpFree.SetComponentEnabled(e, false);
                    lookUpDiagonal.SetComponentEnabled(e, false);
                    lookUpClassic.SetComponentEnabled(e, true);
                    break;
            }
        }
    }

    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct MoveTowardsJob : IJobEntity
    {
        public float dt;

        public readonly float3 MoveTowards(float3 current, float3 target, float maxDistanceDelta)
        {
            float num = target.x - current.x;
            float num2 = target.y - current.y;
            float num3 = target.z - current.z;
            float num4 = num * num + num2 * num2 + num3 * num3;
            if (num4 == 0f || (maxDistanceDelta >= 0f && num4 <= maxDistanceDelta * maxDistanceDelta))
            {
                return target;
            }

            float num5 = math.sqrt(num4);
            return new float3(current.x + num / num5 * maxDistanceDelta, current.y + num2 / num5 * maxDistanceDelta, current.z + num3 / num5 * maxDistanceDelta);
        }

        private void Execute(ref LocalTransform lt, in PlayerTilePosition ptp, in Speed speed)
        {
            lt.Position = MoveTowards(lt.Position, ptp.Target, speed.Value * dt);
        }
    }
} // namespace