using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using Natrium.Gameplay.Components;

namespace Natrium.Gameplay.Systems
{
    /*[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial class PlayerMovementSystem : SystemBase
    {
        public SystemSettingsData mSettings;

        private float dt;
        private float fdt;

        private bool mLogOnce;

        protected override void OnCreate()
        {
            RequireForUpdate<SystemSettingsData>();
            RequireForUpdate<ClientTickRate>();
            RequireForUpdate<MovementSystemExecuteData>();

            mLogOnce = true;

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
            fdt = UnityEngine.Time.fixedDeltaTime;

            mSettings = SystemAPI.GetSingleton<SystemSettingsData>();

            switch (mSettings.movementType)
            {
                case MovementType.Free:
                    FreeMovement();
                    mLogOnce = true;
                    break;
                case MovementType.Full_Tile:
                    FullTileMovement();
                    break;
                case MovementType.Full_Tile_NoDiagonal:
                    FullTileMovementNoDiagonal();
                    break;
                default:
                    if (mLogOnce)
                        Debug.LogError("Movement not handled by " + ToString() + " " + mSettings.movementType.ToString());
                    mLogOnce = false;
                    break;
            }
        }

        private void FreeMovement()
        {
            foreach (var (lt, pd, pid, s) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<PlayerData>, PlayerInputData, SpeedData>()
                        .WithAll<Simulate>())
            {
                //lt.ValueRW.Position += pid.InputAxis * s.value * dt;

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

                //lt.ValueRW.Position = Vector3.MoveTowards(lt.ValueRO.Position, (float3)pd.ValueRO.NextPos, s.value * fdt);
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

                //lt.ValueRW.Position = Vector3.MoveTowards(lt.ValueRO.Position, (float3)pd.ValueRO.NextPos, s.value * fdt);
            }
        }
    }*/


    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class PlayerMovementSystemServer : SystemBase
    {
        private SystemSettingsData _settings;

        private float _dt;

        private bool _logOnce;

        protected override void OnCreate()
        {
            RequireForUpdate<MovementSystemExecute>();
            RequireForUpdate<SystemSettingsData>();

            _logOnce = true;

            base.OnCreate();
        }
        
        protected override void OnUpdate()
        {
            _dt = SystemAPI.Time.DeltaTime;

            _settings = SystemAPI.GetSingleton<SystemSettingsData>();

            switch (_settings.MovementType)
            {
                case MovementType.Free:
                    Free();
                    _logOnce = true;
                    break;
                case MovementType.Diagonal:
                    Diagonal();
                    _logOnce = true;
                    break;
                case MovementType.Classic:
                    Classic();
                    _logOnce = true;
                    break;
                default:
                    if (_logOnce)
                        Debug.LogError("Movement not handled by " + ToString() + " " + _settings.MovementType.ToString());
                    _logOnce = false;
                    break;
            }
        }

        private void Free()
        {
            foreach (var (lt, pd, pid, s) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<Player>, PlayerInput, Speed>()
                        .WithAll<Simulate>())
            {
                lt.ValueRW.Position += pid.InputAxis * s.Value * _dt;

                //When in Free Mode needs to keep track for Hot Swapping between modes.
                pd.ValueRW.NextPos = (int3)math.round(lt.ValueRO.Position);
            }
        }

        private void Diagonal()
        {
            foreach (var (lt, pd, pid, s) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<Player>, PlayerInput, Speed>()
                        .WithAll<Simulate>())
            {

                if (math.distance(lt.ValueRO.Position, pd.ValueRO.NextPos) < s.Value * _dt)
                {
                    lt.ValueRW.Position = pd.ValueRO.NextPos;
                    pd.ValueRW.PreviousPos = pd.ValueRO.NextPos;

                    switch (pid.InputAxis.x)
                    {
                        case > 0:
                            pd.ValueRW.NextPos.x++;
                            break;
                        case < 0:
                            pd.ValueRW.NextPos.x--;
                            break;
                    }

                    switch (pid.InputAxis.z)
                    {
                        case > 0:
                            pd.ValueRW.NextPos.z++;
                            break;
                        case < 0:
                            pd.ValueRW.NextPos.z--;
                            break;
                    }
                }

                lt.ValueRW.Position = Vector3.MoveTowards(lt.ValueRO.Position, (float3)pd.ValueRO.NextPos, s.Value * _dt);
            }
        }

        private void Classic()
        {
            foreach (var (lt, pd, pid, s) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<Player>, PlayerInput, Speed>()
                        .WithAll<Simulate>())
            {
                if (math.distance(lt.ValueRO.Position, pd.ValueRO.NextPos) < s.Value * _dt)
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

                lt.ValueRW.Position = Vector3.MoveTowards(lt.ValueRO.Position, (float3)pd.ValueRO.NextPos, s.Value * _dt);
            }
        }
    }
}
