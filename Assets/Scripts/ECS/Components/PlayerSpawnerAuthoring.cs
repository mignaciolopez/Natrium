using UnityEngine;
using Unity.Entities;

namespace Natrium
{
    [DisallowMultipleComponent]
    public class PlayerSpawnerAuthoring : MonoBehaviour
    {
        public GameObject player;
    }

    public class PlayerAuthoringBaker : Baker<PlayerSpawnerAuthoring>
    {
        public override void Bake(PlayerSpawnerAuthoring authoring)
        {
            Entity entity = this.GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PlayerSpawnerData
            {
                player = GetEntity(authoring.player, TransformUsageFlags.Dynamic)
            });
        }
    }

    public struct PlayerSpawnerData : IComponentData
    {
        public Entity player;
    }
}
