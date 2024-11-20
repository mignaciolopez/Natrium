using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace Natrium.Gameplay.Shared.Components.Debug
{
    public class DebugAuthoring : MonoBehaviour
    {

        public class Baker : Baker<DebugAuthoring>
        {
            public override void Bake(DebugAuthoring authoring)
            {
                var e = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<DebugTag>(e);
            }
        }
    }
    
    [GhostComponent(PrefabType = GhostPrefabType.All, OwnerSendType = SendToOwnerType.All)]
    public struct DebugTag : IComponentData { }
}