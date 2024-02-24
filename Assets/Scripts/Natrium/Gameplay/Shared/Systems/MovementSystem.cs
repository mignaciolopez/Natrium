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
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MovementSystemExecute>();
            state.RequireForUpdate<SystemsSettings>();
        }

        [BurstCompile]
        public void OnStartRunning(ref SystemState state)
        {

        }

        [BurstCompile]
        public void OnStopRunning(ref SystemState state)
        {

        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var dt = SystemAPI.Time.DeltaTime;

            var settings = SystemAPI.GetSingleton<SystemsSettings>();

            switch (settings.MovementType)
            {
                case MovementType.Free:
                    new FreeMovementJob { dt = dt }.Schedule();
                    break;
                case MovementType.Diagonal:
                    new DiagonalMovementJob { dt = dt }.Schedule();
                    break;
                case MovementType.Classic:
                    new ClassicMovementJob { dt = dt }.Schedule();
                    break;
            }
        }

        [BurstCompile]
        public partial struct FreeMovementJob : IJobEntity
        {
            public float dt;

            private void Execute(RefRW<LocalTransform> lt, RefRW<Player> p, PlayerInput pi, Speed s, Simulate sim)
            {
                lt.ValueRW.Position += pi.InputAxis * s.Value * dt;

                //When in Free Mode needs to keep track for Hot Swapping between modes.
                p.ValueRW.NextPos = (int3)math.round(lt.ValueRO.Position);
            }
        }

        [BurstCompile]
        public partial struct DiagonalMovementJob : IJobEntity
        {
            public float dt;

            public static float3 MoveTowards(float3 current, float3 target, float maxDistanceDelta)
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

            private void Execute(RefRW<LocalTransform> lt, RefRW<Player> p, PlayerInput pi, Speed s, Simulate sim)
            {
                if (math.distance(lt.ValueRO.Position, p.ValueRO.NextPos) < s.Value * dt)
                {
                    lt.ValueRW.Position = p.ValueRO.NextPos;
                    p.ValueRW.PreviousPos = p.ValueRO.NextPos;

                    switch (pi.InputAxis.x)
                    {
                        case > 0:
                            p.ValueRW.NextPos.x++;
                            break;
                        case < 0:
                            p.ValueRW.NextPos.x--;
                            break;
                    }

                    switch (pi.InputAxis.z)
                    {
                        case > 0:
                            p.ValueRW.NextPos.z++;
                            break;
                        case < 0:
                            p.ValueRW.NextPos.z--;
                            break;
                    }
                }

                lt.ValueRW.Position = MoveTowards(lt.ValueRW.Position, (float3)p.ValueRO.NextPos, s.Value * dt);
            }
        }

        [BurstCompile]
        public partial struct ClassicMovementJob : IJobEntity
        {
            public float dt;

            public static float3 MoveTowards(float3 current, float3 target, float maxDistanceDelta)
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

            private void Execute(RefRW<LocalTransform> lt, RefRW<Player> p, PlayerInput pi, Speed s, Simulate sim)
            {
                if (math.distance(lt.ValueRO.Position, p.ValueRO.NextPos) < s.Value * dt)
                {
                    lt.ValueRW.Position = p.ValueRO.NextPos;
                    p.ValueRW.PreviousPos = p.ValueRO.NextPos;

                    if (pi.InputAxis.z > 0)
                        p.ValueRW.NextPos.z++;
                    else if (pi.InputAxis.x > 0)
                        p.ValueRW.NextPos.x++;
                    else if (pi.InputAxis.z < 0)
                        p.ValueRW.NextPos.z--;
                    else if (pi.InputAxis.x < 0)
                        p.ValueRW.NextPos.x--;
                }

                lt.ValueRW.Position = MoveTowards(lt.ValueRW.Position, (float3)p.ValueRO.NextPos, s.Value * dt);
            }
        }

    } //MovementSystem
} // namespace