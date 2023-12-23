using UnityEngine;
using Unity.Entities;

namespace Natrium.Gameplay.Server.Components
{
    [DisallowMultipleComponent]
    public class PlayerSpawnerAuthoring : MonoBehaviour
    {
        public GameObject playerPrefab;
    }

    public class PlayerSpawnerBaker : Baker<PlayerSpawnerAuthoring>
    {
        public override void Bake(PlayerSpawnerAuthoring authoring)
        {
            var entity = this.GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PlayerSpawnerData
            {
                PlayerPrefab = GetEntity(authoring.playerPrefab, TransformUsageFlags.Dynamic)
            });
        }
    }

    public struct PlayerSpawnerData : IComponentData
    {
        public Entity PlayerPrefab;
    }
}
