using UnityEngine;
using Unity.Entities;
using Unity.NetCode;

namespace Natrium.Gameplay.Shared.Components
{
    [DisallowMultipleComponent]
    public class AutoDisableAuthoring : MonoBehaviour
    {
        public float time;
        
        public class Baker : Baker<AutoDisableAuthoring>
        {
            public override void Bake(AutoDisableAuthoring authoring)
            {
                var e = GetEntity(TransformUsageFlags.Dynamic);
                if (e != Entity.Null)
                {
                    AddComponent(e, new DisableOnTimer { Value = authoring.time });
                }
            }
        }
    }
    
    public struct DisableOnTimer : IComponentData
    {
        public float Value;
    }

    public struct DisableAtTick : IComponentData
    {
        public NetworkTick Value;
    }
}
