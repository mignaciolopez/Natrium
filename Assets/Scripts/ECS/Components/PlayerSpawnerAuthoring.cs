using UnityEngine;
using Unity.Entities;

namespace Natrium
{
    [DisallowMultipleComponent]
    public class PlayerSpawnerAuthoring : MonoBehaviour
    {
        public GameObject playerPrefab;
    }

    public class PlayerSpawnerAuthoringBaker : Baker<PlayerSpawnerAuthoring>
    {
        public override void Bake(PlayerSpawnerAuthoring authoring)
        {
            Entity entity = this.GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PlayerSpawnerData
            {
                playerPrefab = GetEntity(authoring.playerPrefab, TransformUsageFlags.Dynamic)
            });
        }
    }

    public struct PlayerSpawnerData : IComponentData
    {
        public Entity playerPrefab;
    }
}
