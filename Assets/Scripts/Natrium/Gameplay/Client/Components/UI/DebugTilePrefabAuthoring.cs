using Unity.Entities;
using UnityEngine;

namespace Natrium.Gameplay.Client.Components.UI
{
    public class DebugTilePrefabAuthoring : MonoBehaviour
    {
        public GameObject Prefab;
    }

    public class DebugTilePrefabBaker : Baker<DebugTilePrefabAuthoring>
    {
        public override void Bake(DebugTilePrefabAuthoring authoring)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(e, new PlayerTextPrefab
            {
                Value = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic)
            });
        }
    }

    public struct DebugTilePrefab : IComponentData
    {
        public Entity Value;
    }

    public struct RpcTileDrawnTag : IComponentData { }
    public struct RpcAttackDrawnTag : IComponentData { }
}
