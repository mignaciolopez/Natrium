using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using Natrium.Gameplay.Shared;
using Natrium.Gameplay.Shared.Components;

namespace Natrium.Gameplay.Server.Systems
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class MovementSystem : SystemBase
    {
        private SystemsSettings _settings;

        private float _dt;

        private bool _logOnce;

        protected override void OnCreate()
        {
            base.OnCreate();

            RequireForUpdate<MovementSystemExecute>();
            RequireForUpdate<SystemsSettings>();

            _logOnce = true;
        }

        protected override void OnUpdate()
        {
            _dt = UnityEngine.Time.fixedDeltaTime;

            _settings = SystemAPI.GetSingleton<SystemsSettings>();

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