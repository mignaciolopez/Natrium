using Unity.Entities;
using UnityEngine;

namespace Natrium.Gameplay.Shared.Components
{
    public class RessurectionAuthoring : MonoBehaviour
    {
        private class RessurectionAuthoringBaker : Baker<RessurectionAuthoring>
        {
            public override void Bake(RessurectionAuthoring authoring)
            {
                var e = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<ResurrectTag>(e);
                SetComponentEnabled<ResurrectTag>(e, false);
            }
        }
    }
    
    public struct ResurrectTag : IComponentData, IEnableableComponent { }
}