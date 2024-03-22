using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.NetCode;
using Unity.Mathematics;

namespace Natrium.Gameplay.Shared.Components
{
    [DisallowMultipleComponent]
    public class PlayerAuthoring : MonoBehaviour
    {
    }

    public class PlayerBaker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent<PlayerName>(e);
            AddComponent<Health>(e);
            AddComponent<MaxHealth>(e);
            AddComponent<DamagePoints>(e);
            AddComponent<DebugColor>(e);
        }
    }

    [GhostComponent(PrefabType = GhostPrefabType.All, OwnerSendType = SendToOwnerType.All)]
    public struct PlayerName : IComponentData
    {
        [GhostField]
        public FixedString64Bytes Value;
    }

    [GhostComponent(PrefabType = GhostPrefabType.All, OwnerSendType = SendToOwnerType.SendToOwner)]
    public struct Health : IComponentData
    {
        [GhostField]
        public int Value;
    }

    [GhostComponent(PrefabType = GhostPrefabType.All, OwnerSendType = SendToOwnerType.SendToOwner)]
    public struct MaxHealth : IComponentData
    {
        [GhostField]
        public int Value;
    }

    [GhostComponent(PrefabType = GhostPrefabType.All, OwnerSendType = SendToOwnerType.SendToOwner)]
    public struct DamagePoints : IComponentData
    {
        [GhostField(Quantization = 100)]
        public float Value;
    }

    [GhostComponent(PrefabType = GhostPrefabType.All, OwnerSendType = SendToOwnerType.All)]
    public struct DebugColor : IComponentData
    {
        [GhostField(Quantization = 100)]
        public float3 Value;
    }
}
