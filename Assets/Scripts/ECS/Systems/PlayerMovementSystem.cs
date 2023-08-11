using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.Rendering.DebugUI;

namespace Natrium
{
    public partial class PlayerMovementSystem : SystemBase
    {
        private float3 mMove;
        private float dt;

        protected override void OnCreate()
        {
            base.OnCreate();
            mMove = float3.zero;
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
                        break;
                    case MovementType.Full_Tile:
                        Debug.LogError("Movement not handled by " + ToString() + " " + lapd.movementType.ToString());
                        FullTileMovement(e);
                        break;
                    case MovementType.Full_Tile_NoDiagonal:
                        Debug.LogError("Movement not handled by " + ToString() + " " + lapd.movementType.ToString());
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

            mMove = new float3(Input.GetAxisRaw("Horizontal"), 0.0f, Input.GetAxisRaw("Vertical"));
            mMove = math.normalizesafe(mMove) * dt * speed.value;
            lt.ValueRW.Position += mMove;
        }

        private void FullTileMovement(Entity e)
        {
            var speed = SystemAPI.GetComponent<SpeedData>(e);
            var lt = SystemAPI.GetComponentRW<LocalTransform>(e);
        }

        private void FullTileMovementNoDiagonal(Entity e)
        {
            var speed = SystemAPI.GetComponent<SpeedData>(e);
            var lt = SystemAPI.GetComponentRW<LocalTransform>(e);
        }
    }
}
