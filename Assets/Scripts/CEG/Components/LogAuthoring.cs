using UnityEngine;
using Unity.Entities;
using Unity.Logging;
using Unity.Collections;

namespace CEG.Components
{
    [DisallowMultipleComponent]
    public class LogAuthoring : MonoBehaviour
    {
        public LogLevel Value;
    }

    public class PlayerBaker : Baker<LogAuthoring>
    {
        public override void Bake(LogAuthoring authoring)
        {
            var e = GetEntity(TransformUsageFlags.None);
            AddComponent(e, new LoggingLevel
            {
                Value = authoring.Value
            });
        }
    }

    public struct LoggingLevel : IComponentData
    {
        public LogLevel Value;
    }

    public struct LogData : IComponentData
    {
        public LogLevel logLevel;
        public FixedString512Bytes Message;
        public FixedString128Bytes ClassName;
        public FixedString128Bytes WorldName;
    }
}
