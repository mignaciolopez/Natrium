using UnityEngine;
using Unity.Entities;
using Unity.NetCode;

namespace Natrium.Gameplay.Shared.Components
{
    [DisallowMultipleComponent]
    public class DestroyOnTimerAuthoring : MonoBehaviour
    {
        public float time = 1.0f;
        
        public class Baker : Baker<DestroyOnTimerAuthoring>
        {
            public override void Bake(DestroyOnTimerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new DestroyOnTimer
                {
                    Value = authoring.time
                });
            }
        }
    }

    public struct DestroyOnTimer : IComponentData
    {
        public float Value;
    }
    
    public struct DestroyAtTick : IComponentData
    {
        public NetworkTick Value;
    }

    public struct DestroyEntityTag : IComponentData { }
}
