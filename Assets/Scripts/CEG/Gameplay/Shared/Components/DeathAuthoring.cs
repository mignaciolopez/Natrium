using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace CEG.Gameplay.Shared.Components
{
    public class DeathAuthoring : MonoBehaviour
    {
        private class DeathAuthoringBaker : Baker<DeathAuthoring>
        {
            public override void Bake(DeathAuthoring authoring)
            {
                var e = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<DeathTag>(e);
                SetComponentEnabled<DeathTag>(e, false);
            }
        }
    }

    [GhostComponent(PrefabType = GhostPrefabType.All, OwnerSendType = SendToOwnerType.All)]
    [GhostEnabledBit]
    public struct DeathTag : IComponentData, IEnableableComponent { }
    
    public struct DeathInitialized : IComponentData { }
}