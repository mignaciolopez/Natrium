using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.NetCode;

namespace Natrium.Gameplay.Shared.Components
{
    [DisallowMultipleComponent]
    public class PlayerAuthoring : MonoBehaviour
    {
        public class Baker : Baker<PlayerAuthoring>
        {
            public override void Bake(PlayerAuthoring authoring)
            {
                var e = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<PlayerTag>(e);
                SetComponentEnabled<PlayerTag>(e, true);
                AddComponent<PlayerName>(e);
            }
        }
    }

    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted, OwnerSendType = SendToOwnerType.All)]
    [GhostEnabledBit]
    public struct PlayerTag : IComponentData, IEnableableComponent { }
    public struct InitializedPlayerTag : IComponentData { }

    [GhostComponent(PrefabType = GhostPrefabType.All, OwnerSendType = SendToOwnerType.All)]
    public struct PlayerName : IComponentData
    {
        [GhostField] public FixedString64Bytes Value;
    }
}
