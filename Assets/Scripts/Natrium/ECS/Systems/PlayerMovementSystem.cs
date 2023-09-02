using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Natrium.ECS.Components;

namespace Natrium.Ecs.Systems
{
    public partial class PlayerMovementSystem : SystemBase
    {
        private float _dt;
        private float _previousDT;
        private float3 _previousPos;
        private float3 _nextPos;

        protected override void OnCreate()
        {
            base.OnCreate();
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            _dt = SystemAPI.Time.DeltaTime;
            _previousDT = _dt;
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
            _dt = SystemAPI.Time.DeltaTime;

            foreach (var ( lt, lapd, e) in SystemAPI.Query<RefRW<LocalTransform>, LocalActivePlayerData>().WithEntityAccess())
            {
                switch (lapd.movementType)
                {
                    case MovementType.Free:
                        FreeMovement(e);

                        //When in Free Mode needs to keep track for Hot Swapping between modes.
                        _nextPos = math.round(lt.ValueRW.Position);
                        break;
                    case MovementType.FullTile:
                        FullTileMovement(e);
                        break;
                    case MovementType.FullTileNoDiagonal:
                        FullTileMovementNoDiagonal(e);
                        break;
                    default:
                        Debug.LogError($"Movement not handled by {this} {lapd.movementType}");
                        break;
                }
            }

            _previousDT = _dt;
        }

        private void FreeMovement(Entity e)
        {
            var speed = SystemAPI.GetComponent<SpeedData>(e);
            var lt = SystemAPI.GetComponentRW<LocalTransform>(e);

            var move = new float3(Input.GetAxis("JHorizontal"), 0.0f, Input.GetAxis("JVertical"));
            if (move is { x: 0, z: 0 })
            {
                move = new float3(Input.GetAxisRaw("Horizontal"), 0.0f, Input.GetAxisRaw("Vertical"));
                move = math.normalizesafe(move);
            }

            move *= _dt * speed.value;

            lt.ValueRW.Position +=  move;
        }

        private void FullTileMovement(Entity e)
        {
            var speed = SystemAPI.GetComponent<SpeedData>(e);
            var lt = SystemAPI.GetComponentRW<LocalTransform>(e);

            if (math.distance(lt.ValueRO.Position, _nextPos) < speed.value * _previousDT)
            {
                _previousPos = _nextPos;

                var move = new float3(Input.GetAxis("JHorizontal"), 0.0f, Input.GetAxis("JVertical"));
                if (move is { x: 0, z: 0 })
                    move = new float3(Input.GetAxisRaw("Horizontal"), 0.0f, Input.GetAxisRaw("Vertical"));

                switch (move.x)
                {
                    case > 0:
                        _nextPos.x++;
                        break;
                    case < 0:
                        _nextPos.x--;
                        break;
                }

                switch (move.z)
                {
                    case > 0:
                        _nextPos.z++;
                        break;
                    case < 0:
                        _nextPos.z--;
                        break;
                }
            }

            lt.ValueRW.Position = Vector3.MoveTowards(lt.ValueRO.Position, _nextPos, speed.value * _dt);
        }

        private void FullTileMovementNoDiagonal(Entity e)
        {
            var speed = SystemAPI.GetComponent<SpeedData>(e);
            var lt = SystemAPI.GetComponentRW<LocalTransform>(e);

            if (math.distance(lt.ValueRO.Position, _nextPos) < speed.value * _previousDT)
            {
                _previousPos = _nextPos;

                var move = new float3(Input.GetAxis("JHorizontal"), 0.0f, Input.GetAxis("JVertical"));
                if (move is { x: 0, z: 0 })
                    move = new float3(Input.GetAxisRaw("Horizontal"), 0.0f, Input.GetAxisRaw("Vertical"));

                if (move.z > 0)
                    _nextPos.z++;
                else if (move.x > 0)
                    _nextPos.x++;
                else if (move.z < 0)
                    _nextPos.z--;
                else if (move.x < 0)
                    _nextPos.x--;
            }

            lt.ValueRW.Position = Vector3.MoveTowards(lt.ValueRO.Position, _nextPos, speed.value * _dt);
        }
    }
}
