using UnityEngine;
using Unity.Entities;
using Unity.NetCode;

namespace Natrium.Gameplay.Shared.Components
{
    [DisallowMultipleComponent]
    public class DamageDealerAuthoring : MonoBehaviour
    {
    }

    public class DamageDealerBaker : Baker<DamageDealerAuthoring>
    {
        public override void Bake(DamageDealerAuthoring authoring)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);
            if (e != Entity.Null)
            {
                AddComponent<DamageDealerTag>(e);
                AddComponent<DamagePoints>(e);
            }
        }
    }

    public struct DamageDealerTag : IComponentData { }

    [GhostComponent(PrefabType = GhostPrefabType.All, OwnerSendType = SendToOwnerType.SendToOwner)]
    public struct DamagePoints : IComponentData
    {
        [GhostField] public int Value;
    }
}