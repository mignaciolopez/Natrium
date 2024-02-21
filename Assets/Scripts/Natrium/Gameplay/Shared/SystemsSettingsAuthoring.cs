using UnityEngine;
using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;

namespace Natrium.Gameplay.Shared
{
    public enum MovementType
    {
        Classic = 0,
        Diagonal,
        Free
    }



    [DisallowMultipleComponent]
    public class SystemsSettingsAuthoring : MonoBehaviour
    {
        #region ConnectionSettings
        [Header("ConnectionSettings")]
        public string FQDN = "localhost";
        public ushort Port = 7979;
        #endregion //ConnectionSettings

        #region MovementSystem
        [Header("MovementSystem")]
            public MovementType movementType = MovementType.Free;

        #endregion

        #region NetCode
        [Header("NetCode")]
            public bool addCustomClientTickRate;

            [Tooltip("Good ranges: [0.075 - 0.2]")]
            public float commandAgeCorrectionFraction = 0.1f;

            [Tooltip("Must be in range (0, 1)")]
            public float interpolationDelayCorrectionFraction = 0.1f;
            
            public float interpolationDelayJitterScale = 1.25f;

            [Tooltip("Good ranges: [0.10 - 0.3]")]
            public float interpolationDelayMaxDeltaTicksFraction = 0.1f;

            public bool useInterpolationTimeMS;
            public uint interpolationTimeMS ;

            public bool useInterpolationTimeNetTicks;
            public uint interpolationTimeNetTicks;

            [Tooltip("Must be greater that 1.0. Default: 1.1")]
            public float interpolationTimeScaleMax = 1.1f;

            [Tooltip("Must be in range (0, 1) Default: 0.85")]
            public float interpolationTimeScaleMin = 0.85f;

            public bool useMaxExtrapolationTimeSimTicks;
            public uint maxExtrapolationTimeSimTicks;

            public bool useMaxPredictAheadTimeMS;
            public uint maxPredictAheadTimeMS;

            public bool useMaxPredictionStepBatchSizeFirstTimeTick;
            public int maxPredictionStepBatchSizeFirstTimeTick;

            public bool useMaxPredictionStepBatchSizeRepeatedTick;
            public int maxPredictionStepBatchSizeRepeatedTick;

            [Tooltip("Good Range: (1.05 - 1.2)")]
            public float predictionTimeScaleMax = 1.1f;

            [Tooltip("Good Range: (0.8 - 0.95)")]
            public float predictionTimeScaleMin = 0.9f;

            public bool useTargetCommandSlack;
            public uint targetCommandSlack;

        #endregion
    }

    public class SystemsSettingsBaker : Baker<SystemsSettingsAuthoring>
    {
        public override void Bake(SystemsSettingsAuthoring authoring)
        {
            var e = GetEntity(TransformUsageFlags.None);
            {
                AddComponent(e, new SystemsSettings
                {
                    #region ConnectionSettings
                    FQDN = authoring.FQDN,
                    Port = authoring.Port,
                    #endregion //ConnectionSettings

                    #region MovementSystem
                    MovementType = authoring.movementType
                    #endregion //MovementSystem
                });
            }

            #region NetCode
            {
                if (!authoring.addCustomClientTickRate) return;
                var ctr = new ClientTickRate
                {
                    CommandAgeCorrectionFraction = authoring.commandAgeCorrectionFraction,
                    InterpolationDelayCorrectionFraction = authoring.interpolationDelayCorrectionFraction,
                    InterpolationDelayJitterScale = authoring.interpolationDelayJitterScale,
                    InterpolationDelayMaxDeltaTicksFraction = authoring.interpolationDelayMaxDeltaTicksFraction
                };

                if (authoring.useInterpolationTimeMS) ctr.InterpolationTimeMS = authoring.interpolationTimeMS;

                if (authoring.useInterpolationTimeNetTicks) ctr.InterpolationTimeNetTicks = authoring.interpolationTimeNetTicks;

                ctr.InterpolationTimeScaleMax = authoring.interpolationTimeScaleMax;
                ctr.InterpolationTimeScaleMin = authoring.interpolationTimeScaleMin;

                if (authoring.useMaxExtrapolationTimeSimTicks) ctr.MaxExtrapolationTimeSimTicks = authoring.maxExtrapolationTimeSimTicks;

                if (authoring.useMaxPredictAheadTimeMS) ctr.MaxPredictAheadTimeMS = authoring.maxPredictAheadTimeMS;

                if (authoring.useMaxPredictionStepBatchSizeFirstTimeTick) ctr.MaxPredictionStepBatchSizeFirstTimeTick = authoring.maxPredictionStepBatchSizeFirstTimeTick;

                if (authoring.useMaxPredictionStepBatchSizeRepeatedTick) ctr.MaxPredictionStepBatchSizeRepeatedTick = authoring.maxPredictionStepBatchSizeRepeatedTick;

                ctr.PredictionTimeScaleMax = authoring.predictionTimeScaleMax;
                ctr.PredictionTimeScaleMin = authoring.predictionTimeScaleMin;

                if (authoring.useTargetCommandSlack) ctr.TargetCommandSlack = authoring.targetCommandSlack;

                AddComponent(e, ctr);
            }
            #endregion
        }
    }

    public struct SystemsSettings : IComponentData
    {
        #region ConnectionSettings
        public FixedString32Bytes FQDN;
        public ushort Port;
        #endregion //ConnectionSettings

        #region MovementSystem
        public MovementType MovementType;
        #endregion //MovementSystem
    }
}
