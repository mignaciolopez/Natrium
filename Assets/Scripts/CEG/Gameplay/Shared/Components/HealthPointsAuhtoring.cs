using Unity.Entities;
using UnityEngine;
using Unity.NetCode;

namespace CEG.Gameplay.Shared.Components
{
    public class HealthPointsAuhtoring : MonoBehaviour
    {
        public int healthPoints = 1;

        private class HealthPointsAuhtoringBaker : Baker<HealthPointsAuhtoring>
        {
            public override void Bake(HealthPointsAuhtoring authoring)
            {
                var e = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(e, new HealthPoints
                {
                    MaxValue = authoring.healthPoints
                });
            }
        }
    }
    
    [GhostComponent(PrefabType = GhostPrefabType.All, OwnerSendType = SendToOwnerType.All)]
    public struct HealthPoints : IComponentData
    {
        public int MaxValue;
        [GhostField] public int Value;
    }
}