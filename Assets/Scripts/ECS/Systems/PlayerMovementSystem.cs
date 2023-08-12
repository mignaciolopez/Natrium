using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

namespace Natrium
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    public partial class PlayerMovementSystem : SystemBase
    {
        private float dt;
        private float mPreviousDT;
        private float3 mPreviousPos;
        private float3 mNextPos;

        protected override void OnCreate()
        {
            base.OnCreate();
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            dt = SystemAPI.Time.DeltaTime;
            mPreviousDT = dt;
        }

        protected override void OnStopRunning()
        {
            base.OnStopRunning();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
        protected override void OnUpdate()
        {
            dt = SystemAPI.Time.DeltaTime;

            foreach (var (pid, lt, e) in SystemAPI.Query<RefRO<PlayerInputData>, RefRW<LocalTransform>>()
                .WithAll<Simulate>()
                .WithEntityAccess())
            {
                switch (pid.ValueRO.movementType)
                {
                    case MovementType.Free:
                        FreeMovement(e);

                        //When in Free Mode needs to keep track for Hot Swapping between modes.
                        mNextPos = math.round(lt.ValueRO.Position);
                        break;
                    case MovementType.Full_Tile:
                        FullTileMovement(e);
                        break;
                    case MovementType.Full_Tile_NoDiagonal:
                        FullTileMovementNoDiagonal(e);
                        break;
                    default:
                        Debug.LogError("Movement not handled by " + ToString() + " " + pid.ValueRO.movementType.ToString());
                        break;
                }
            }

            mPreviousDT = dt;
        }

        private void FreeMovement(Entity e)
        {
            var speed = SystemAPI.GetComponent<SpeedData>(e);
            var lt = SystemAPI.GetComponentRW<LocalTransform>(e);
            var pid = SystemAPI.GetComponent<PlayerInputData>(e);

            lt.ValueRW.Position += pid.InputAxis * speed.value * dt;
        }

        private void FullTileMovement(Entity e)
        {
            var speed = SystemAPI.GetComponent<SpeedData>(e);
            var lt = SystemAPI.GetComponentRW<LocalTransform>(e);
            var pid = SystemAPI.GetComponent<PlayerInputData>(e);

            if (math.distance(lt.ValueRO.Position, mNextPos) < speed.value * mPreviousDT)
            {
                mPreviousPos = mNextPos;

                if (pid.InputAxis.x > 0)
                    mNextPos.x++;
                else if (pid.InputAxis.x < 0)
                    mNextPos.x--;

                if (pid.InputAxis.z > 0)
                    mNextPos.z++;
                else if (pid.InputAxis.z < 0)
                    mNextPos.z--;
            }

            lt.ValueRW.Position = Vector3.MoveTowards(lt.ValueRO.Position, mNextPos, speed.value * dt);
        }

        private void FullTileMovementNoDiagonal(Entity e)
        {
            var speed = SystemAPI.GetComponent<SpeedData>(e);
            var lt = SystemAPI.GetComponentRW<LocalTransform>(e);
            var pid = SystemAPI.GetComponent<PlayerInputData>(e);

            if (math.distance(lt.ValueRO.Position, mNextPos) < speed.value * mPreviousDT)
            {
                mPreviousPos = mNextPos;

                if (pid.InputAxis.z > 0)
                    mNextPos.z++;
                else if (pid.InputAxis.x > 0)
                    mNextPos.x++;
                else if (pid.InputAxis.z < 0)
                    mNextPos.z--;
                else if (pid.InputAxis.x < 0)
                    mNextPos.x--;
            }

            lt.ValueRW.Position = Vector3.MoveTowards(lt.ValueRO.Position, mNextPos, speed.value * dt);
        }
    }
}
