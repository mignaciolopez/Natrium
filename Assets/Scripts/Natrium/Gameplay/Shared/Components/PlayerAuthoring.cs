using Natrium.Shared.Extensions;
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
        public Color deathColor;
        
        public class Baker : Baker<PlayerAuthoring>
        {
            public override void Bake(PlayerAuthoring authoring)
            {
                var e = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<PlayerTag>(e);
                AddComponent<PlayerName>(e);
                AddComponent(e, new DebugColor
                {
                    DeathValue = authoring.deathColor.ToFloat4()
                });
            }
        }
    }

    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted, OwnerSendType = SendToOwnerType.SendToOwner)]
    public struct PlayerTag : IComponentData { }
    public struct InitialezedPlayerTag : IComponentData { }

    [GhostComponent(PrefabType = GhostPrefabType.All, OwnerSendType = SendToOwnerType.All)]
    public struct PlayerName : IComponentData
    {
        [GhostField]
        public FixedString64Bytes Value;
    }

    [GhostComponent(PrefabType = GhostPrefabType.All, OwnerSendType = SendToOwnerType.All)]
    public struct DebugColor : IComponentData
    {
        [GhostField(Quantization = 100)]
        public float4 Value;
        
        [GhostField(Quantization = 100)]
        public float4 StartValue;
        
        [GhostField(Quantization = 100)]
        public float4 DeathValue;
    }
}
