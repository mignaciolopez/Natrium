using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Rendering;
using UnityEngine;

namespace Natrium.Gameplay.Shared.Components
{
    [RequireComponent(typeof(GhostAuthoringInspectionComponent))]
    public class SkinAuthoring : MonoBehaviour
    {
        public Color deathColor;
        private class SkinAuthoringBaker : Baker<SkinAuthoring>
        {
            public override void Bake(SkinAuthoring authoring)
            {
                var e = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<MaterialPropertyBaseColor>(e);
                AddComponentObject(e, new ColorDeath
                {
                    Value = authoring.deathColor
                });
                AddComponentObject(e, new ColorAlive());
            }
        }
    }

    [GhostComponent(PrefabType = GhostPrefabType.All, OwnerSendType = SendToOwnerType.All)]
    [MaterialProperty("_BaseColor")]
    public struct MaterialPropertyBaseColor : IComponentData
    {
        [GhostField] public float4 Value;
    }
    
    public class ColorDeath : IComponentData
    {
        public Color Value;
    }
    
    public class ColorAlive : IComponentData
    {
        public Color Value;
    }
}