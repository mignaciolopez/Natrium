using UnityEngine;
using Unity.Entities;

namespace Natrium.Gameplay.Server.Components
{
    [DisallowMultipleComponent]
    public class PrefabsAuthoring : MonoBehaviour
    {
        public GameObject PlayerPrefab;
    }

    public class PlayerSpawnerBaker : Baker<PrefabsAuthoring>
    {
        public override void Bake(PrefabsAuthoring authoring)
        {
            var e = this.GetEntity(TransformUsageFlags.Dynamic);
            if(e != Entity.Null)
            {
                AddComponent(e, new PlayerPrefab
                {
                    Value = GetEntity(authoring.PlayerPrefab, TransformUsageFlags.Dynamic)
                });
            }
        }
    }

    public struct PlayerPrefab : IComponentData
    {
        public Entity Value;
    }
}
