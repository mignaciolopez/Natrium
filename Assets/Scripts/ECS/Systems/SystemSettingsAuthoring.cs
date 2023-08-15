using UnityEngine;
using Unity.Entities;
using Unity.NetCode;
using Unity.Mathematics;
using System.ComponentModel;
using Unity.VisualScripting;

namespace Natrium
{
    public enum MovementType
    {
        Free = 0,
        Full_Tile,
        Full_Tile_NoDiagonal
    }

    [DisallowMultipleComponent]
    public class SystemSettingsAuthoring : MonoBehaviour
    {
        #region Movement System
        [Header("Movement System")]
            public MovementType movementType = MovementType.Free;

        #endregion

        #region NetCode
        [Header("NetCode")]
            public bool AddCustomClientTickRate = false;

            [Tooltip("Good ranges: [0.075 - 0.2]")]
            public float CommandAgeCorrectionFraction = 0.1f;

            [Tooltip("Must be in range (0, 1)")]
            public float InterpolationDelayCorrectionFraction = 0.1f;
            
            public float InterpolationDelayJitterScale = 1.25f;

            [Tooltip("Good ranges: [0.10 - 0.3]")]
            public float InterpolationDelayMaxDeltaTicksFraction = 0.1f;

            public bool UseInterpolationTimeMS = false;
            public uint InterpolationTimeMS = 0;

            public bool UseInterpolationTimeNetTicks = false;
            public uint InterpolationTimeNetTicks = 0;

            [Tooltip("Must be greater that 1.0. Default: 1.1")]
            public float InterpolationTimeScaleMax = 1.1f;

            [Tooltip("Must be in range (0, 1) Default: 0.85")]
            public float InterpolationTimeScaleMin = 0.85f;

            public bool UseMaxExtrapolationTimeSimTicks = false;
            public uint MaxExtrapolationTimeSimTicks = 0;

            public bool UseMaxPredictAheadTimeMS = false;
            public uint MaxPredictAheadTimeMS = 0;

            public bool UseMaxPredictionStepBatchSizeFirstTimeTick = false;
            public int MaxPredictionStepBatchSizeFirstTimeTick = 0;

            public bool UseMaxPredictionStepBatchSizeRepeatedTick = false;
            public int MaxPredictionStepBatchSizeRepeatedTick = 0;

            [Tooltip("Good Range: (1.05 - 1.2)")]
            public float PredictionTimeScaleMax = 1.1f;

            [Tooltip("Good Range: (0.8 - 0.95)")]
            public float PredictionTimeScaleMin = 0.9f;

            public bool UseTargetCommandSlack = false;
            public uint TargetCommandSlack = 0;

        #endregion
    }

    public class SystemSettingsBaker : Baker<SystemSettingsAuthoring>
    {
        public override void Bake(SystemSettingsAuthoring authoring)
        {
            Entity e = GetEntity(TransformUsageFlags.None);

            #region Movement System
            {
                AddComponent(e, new SystemSettingsData
                {
                    movementType = authoring.movementType
                });
            }
            #endregion

            #region NetCode
            {
                if (authoring.AddCustomClientTickRate)
                {
                    var ctr = new ClientTickRate();

                    ctr.CommandAgeCorrectionFraction = authoring.CommandAgeCorrectionFraction;
                    ctr.InterpolationDelayCorrectionFraction = authoring.InterpolationDelayCorrectionFraction;
                    ctr.InterpolationDelayJitterScale = authoring.InterpolationDelayJitterScale;
                    ctr.InterpolationDelayMaxDeltaTicksFraction = authoring.InterpolationDelayMaxDeltaTicksFraction;

                    if (authoring.UseInterpolationTimeMS) ctr.InterpolationTimeMS = authoring.InterpolationTimeMS;

                    if (authoring.UseInterpolationTimeNetTicks) ctr.InterpolationTimeNetTicks = authoring.InterpolationTimeNetTicks;

                    ctr.InterpolationTimeScaleMax = authoring.InterpolationTimeScaleMax;
                    ctr.InterpolationTimeScaleMin = authoring.InterpolationTimeScaleMin;

                    if (authoring.UseMaxExtrapolationTimeSimTicks) ctr.MaxExtrapolationTimeSimTicks = authoring.MaxExtrapolationTimeSimTicks;

                    if (authoring.UseMaxPredictAheadTimeMS) ctr.MaxPredictAheadTimeMS = authoring.MaxPredictAheadTimeMS;

                    if (authoring.UseMaxPredictionStepBatchSizeFirstTimeTick) ctr.MaxPredictionStepBatchSizeFirstTimeTick = authoring.MaxPredictionStepBatchSizeFirstTimeTick;

                    if (authoring.UseMaxPredictionStepBatchSizeRepeatedTick) ctr.MaxPredictionStepBatchSizeRepeatedTick = authoring.MaxPredictionStepBatchSizeRepeatedTick;

                    ctr.PredictionTimeScaleMax = authoring.PredictionTimeScaleMax;
                    ctr.PredictionTimeScaleMin = authoring.PredictionTimeScaleMin;

                    if (authoring.UseTargetCommandSlack) ctr.TargetCommandSlack = authoring.TargetCommandSlack;

                    AddComponent(e, ctr);
                }
            }
            #endregion
        }
    }

    public struct SystemSettingsData : IComponentData
    {
        public MovementType movementType;
        
    }
}
