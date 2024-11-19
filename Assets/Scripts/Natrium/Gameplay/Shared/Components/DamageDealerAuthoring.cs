using UnityEngine;
using Unity.Entities;
using Unity.NetCode;

namespace Natrium.Gameplay.Shared.Components
{
    [DisallowMultipleComponent]
    public class DamageDealerAuthoring : MonoBehaviour
    {
        public float damagePoints = 1.0f;
    }

    public class DamageDealerBaker : Baker<DamageDealerAuthoring>
    {
        public override void Bake(DamageDealerAuthoring authoring)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);
            if (e != Entity.Null)
            {
                AddComponent<DamageDealerTag>(e);
                AddComponent(e, new DamagePoints
                {
                    Value = authoring.damagePoints
                });
            }
        }
    }

    public struct DamageDealerTag : IComponentData { }

    [GhostComponent(PrefabType = GhostPrefabType.All, OwnerSendType = SendToOwnerType.All)]
    public struct DamagePoints : IComponentData
    {
        [GhostField] public float Value;
    }
}