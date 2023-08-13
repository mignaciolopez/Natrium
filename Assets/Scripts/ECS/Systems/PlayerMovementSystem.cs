using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;

namespace Natrium
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    public partial class PlayerMovementSystem : SystemBase
    {
        public SystemSettingsData mSettings;

        private float dt;
        private float fdt;

        protected override void OnCreate()
        {
            base.OnCreate();
        }

        protected override void OnStartRunning()
        {
            RequireForUpdate<SystemSettingsData>();
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
            fdt = UnityEngine.Time.fixedDeltaTime;

            mSettings = SystemAPI.GetSingleton<SystemSettingsData>();

            switch (mSettings.movementType)
            {
                case MovementType.Free:
                    FreeMovement();
                    break;
                case MovementType.Full_Tile:
                    FullTileMovement();
                    break;
                case MovementType.Full_Tile_NoDiagonal:
                    FullTileMovementNoDiagonal();
                    break;
                default:
                    Debug.LogError("Movement not handled by " + ToString() + " " + mSettings.movementType.ToString());
                    break;
            }
        }

        private void FreeMovement()
        {
            foreach (var (lt, pd, pid, s) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<PlayerData>, PlayerInputData, SpeedData>()
                        .WithAll<Simulate>())
            {
                lt.ValueRW.Position += pid.InputAxis * s.value * dt;

                //When in Free Mode needs to keep track for Hot Swapping between modes.
                pd.ValueRW.NextPos = (int3)math.round(lt.ValueRO.Position);
            }
        }

        private void FullTileMovement()
        {
            foreach (var (lt, pd, pid, s) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<PlayerData>, PlayerInputData, SpeedData>()
                        .WithAll<Simulate>())
            {

                if (math.distance(lt.ValueRO.Position, pd.ValueRO.NextPos) < s.value * fdt)
                {
                    lt.ValueRW.Position = pd.ValueRO.NextPos;
                    pd.ValueRW.PreviousPos = pd.ValueRO.NextPos;

                    if (pid.InputAxis.x > 0)
                        pd.ValueRW.NextPos.x++;
                    else if (pid.InputAxis.x < 0)
                        pd.ValueRW.NextPos.x--;

                    if (pid.InputAxis.z > 0)
                        pd.ValueRW.NextPos.z++;
                    else if (pid.InputAxis.z < 0)
                        pd.ValueRW.NextPos.z--;
                }

                lt.ValueRW.Position = Vector3.MoveTowards(lt.ValueRO.Position, (float3)pd.ValueRO.NextPos, s.value * fdt);
            }
        }

        private void FullTileMovementNoDiagonal()
        {
            foreach (var (lt, pd, pid, s) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<PlayerData>, PlayerInputData, SpeedData>()
                        .WithAll<Simulate>())
            {
                if (math.distance(lt.ValueRO.Position, pd.ValueRO.NextPos) < s.value * fdt)
                {
                    lt.ValueRW.Position = pd.ValueRO.NextPos;
                    pd.ValueRW.PreviousPos = pd.ValueRO.NextPos;

                    if (pid.InputAxis.z > 0)
                        pd.ValueRW.NextPos.z++;
                    else if (pid.InputAxis.x > 0)
                        pd.ValueRW.NextPos.x++;
                    else if (pid.InputAxis.z < 0)
                        pd.ValueRW.NextPos.z--;
                    else if (pid.InputAxis.x < 0)
                        pd.ValueRW.NextPos.x--;
                }

                lt.ValueRW.Position = Vector3.MoveTowards(lt.ValueRO.Position, (float3)pd.ValueRO.NextPos, s.value * fdt);
            }
        }
    }
}
