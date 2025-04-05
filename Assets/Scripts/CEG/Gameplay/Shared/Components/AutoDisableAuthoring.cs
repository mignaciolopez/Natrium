using UnityEngine;
using Unity.Entities;
using Unity.NetCode;

namespace CEG.Gameplay.Shared.Components
{
    [DisallowMultipleComponent]
    public class AutoDisableAuthoring : MonoBehaviour
    {
        public float time;
        
        public class Baker : Baker<AutoDisableAuthoring>
        {
            public override void Bake(AutoDisableAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                if (entity == Entity.Null)
                {
                    return;
                }
                AddComponent(entity, new DisableOnTimer
                {
                    Value = authoring.time
                });
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
