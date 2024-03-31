using UnityEngine;
using Unity.Entities;
using Unity.NetCode;

namespace Natrium.Gameplay.Shared.Components
{
    [DisallowMultipleComponent]
    public class DebugTileAuthoring : MonoBehaviour
    {
    }

    public class DebugTileBaker : Baker<DebugTileAuthoring>
    {
        public override void Bake(DebugTileAuthoring authoring)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);
            if (e != Entity.Null)
            {
                AddComponent<DebugTileTag>(e);
                AddComponent<DebugColor>(e);
            }
        }
    }

    public struct DebugTileTag : IComponentData { }
}
