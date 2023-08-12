using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Natrium
{
    public partial class PlayerMovementSystem : SystemBase
    {
        private float dt;

        private float3 mPreviousPos;
        private float3 mNextPos;

        protected override void OnCreate()
        {
            base.OnCreate();
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
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

            foreach ((RefRW<LocalTransform> lt, LocalActivePlayerData lapd, Entity e) in
                SystemAPI.Query<RefRW<LocalTransform>, LocalActivePlayerData>().WithEntityAccess())
            {
                switch (lapd.movementType)
                {
                    case MovementType.Free:
                        FreeMovement(e);

                        //When in Free Mode needs to keep track for Hot Swapping between modes.
                        mNextPos = math.round(lt.ValueRW.Position);
                        break;
                    case MovementType.Full_Tile:
                        FullTileMovement(e);
                        break;
                    case MovementType.Full_Tile_NoDiagonal:
                        FullTileMovementNoDiagonal(e);
                        break;
                    default:
                        Debug.LogError("Movement not handled by " + ToString() + " " + lapd.movementType.ToString());
                        break;
                }
            }
        }

        private void FreeMovement(Entity e)
        {
            var speed = SystemAPI.GetComponent<SpeedData>(e);
            var lt = SystemAPI.GetComponentRW<LocalTransform>(e);

            float3 move = new float3(Input.GetAxis("JHorizontal"), 0.0f, Input.GetAxis("JVertical"));
            if (move.x == 0 && move.z == 0)
            {
                move = new float3(Input.GetAxisRaw("Horizontal"), 0.0f, Input.GetAxisRaw("Vertical"));
                move = math.normalizesafe(move);
            }

            move *= dt * speed.value;

            lt.ValueRW.Position +=  move;
        }

        private void FullTileMovement(Entity e)
        {
            var speed = SystemAPI.GetComponent<SpeedData>(e);
            var lt = SystemAPI.GetComponentRW<LocalTransform>(e);
            var lapd = SystemAPI.GetComponentRW<LocalActivePlayerData>(e);

            CalculateAutoDistance(e);

            if (math.distance(lt.ValueRO.Position, mNextPos) <= lapd.ValueRO.minDistanceInput)
            {
                mPreviousPos = mNextPos;

                float3 move = new float3(Input.GetAxis("JHorizontal"), 0.0f, Input.GetAxis("JVertical"));
                if (move.x == 0 && move.z == 0)
                    move = new float3(Input.GetAxisRaw("Horizontal"), 0.0f, Input.GetAxisRaw("Vertical"));

                if (move.x > 0)
                    mNextPos.x++;
                else if (move.x < 0)
                    mNextPos.x--;

                if (move.z > 0)
                    mNextPos.z++;
                else if (move.z < 0)
                    mNextPos.z--;
            }

            lt.ValueRW.Position = Vector3.MoveTowards(lt.ValueRO.Position, mNextPos, speed.value * dt);
        }

        private void FullTileMovementNoDiagonal(Entity e)
        {
            var speed = SystemAPI.GetComponent<SpeedData>(e);
            var lt = SystemAPI.GetComponentRW<LocalTransform>(e);
            var lapd = SystemAPI.GetComponentRW<LocalActivePlayerData>(e);

            CalculateAutoDistance(e);

            if (math.distance(lt.ValueRO.Position, mNextPos) <= lapd.ValueRO.minDistanceInput)
            {
                mPreviousPos = mNextPos;

                float3 move = new float3(Input.GetAxis("JHorizontal"), 0.0f, Input.GetAxis("JVertical"));
                if (move.x == 0 && move.z == 0)
                    move = new float3(Input.GetAxisRaw("Horizontal"), 0.0f, Input.GetAxisRaw("Vertical"));

                if (move.z > 0)
                    mNextPos.z++;
                else if (move.x > 0)
                    mNextPos.x++;
                else if (move.z < 0)
                    mNextPos.z--;
                else if (move.x < 0)
                    mNextPos.x--;
            }

            lt.ValueRW.Position = Vector3.MoveTowards(lt.ValueRO.Position, mNextPos, speed.value * dt);
        }

        private void CalculateAutoDistance(Entity e)
        {
            var lapd = SystemAPI.GetComponentRW<LocalActivePlayerData>(e);

            if (lapd.ValueRO.autoDistance)
                lapd.ValueRW.minDistanceInput = 0.05f; //TODO: faking a value for now
        }
    }
}
