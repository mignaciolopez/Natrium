using UnityEngine;
using Unity.Entities;
using Unity.NetCode;

namespace Natrium.Gameplay.Shared.Components
{
    [DisallowMultipleComponent]
    public class TimerAuhtoring : MonoBehaviour
    {
        public float Time;
    }

    public class TimerBaker : Baker<TimerAuhtoring>
    {
        public override void Bake(TimerAuhtoring authoring)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);
            if (e != Entity.Null)
            {
                AddComponent(e, new DestroyOnTimer { Value = authoring.Time });
                AddComponent(e, new DestroyAtTick { });
            }
        }
    }

    public struct DestroyOnTimer : IComponentData
    {
        public float Value;
    }

    public struct DestroyAtTick : IComponentData
    {
        [GhostField] public NetworkTick Value;
    }

    public struct DestroyEntityTag : IComponentData { }
}
