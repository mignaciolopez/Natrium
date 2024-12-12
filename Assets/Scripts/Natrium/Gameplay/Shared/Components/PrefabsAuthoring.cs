using UnityEngine;
using Unity.Entities;

namespace Natrium.Gameplay.Shared.Components
{
    [DisallowMultipleComponent]
    public class PrefabsAuthoring : MonoBehaviour
    {
        public GameObject playerPrefab;
        
        public class Baker : Baker<PrefabsAuthoring>
        {
            public override void Bake(PrefabsAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                if (entity == Entity.Null)
                {
                    return;
                }
                
                AddComponent(entity, new PlayerPrefab
                {
                    Value = GetEntity(authoring.playerPrefab, TransformUsageFlags.Dynamic)
                });
            }
        }
    }

    public struct PlayerPrefab : IComponentData
    {
        public Entity Value;
    }
}
