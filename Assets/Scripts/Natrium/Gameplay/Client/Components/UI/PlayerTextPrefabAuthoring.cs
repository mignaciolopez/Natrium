using Unity.Entities;
using UnityEngine;

namespace Natrium.Gameplay.Client.Components.UI
{
    public class PlayerTextPrefabAuthoring : MonoBehaviour
    {
        public GameObject Prefab;
    }

    public class PlayerTextPrefabBaker : Baker<PlayerTextPrefabAuthoring>
    {
        public override void Bake(PlayerTextPrefabAuthoring authoring)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(e, new PlayerTextPrefab
            {
                Value = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic)
            });
        }
    }

    public struct PlayerTextPrefab : IComponentData
    {
        public Entity Value;
    }

    public struct PlayerTextDrawnTag : IComponentData { }
}
