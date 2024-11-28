using Unity.Entities;
using UnityEngine;

namespace Natrium.Gameplay.Client.Components.UI
{
    public class PlayerTextPrefabAuthoring : MonoBehaviour
    {
        public GameObject prefab;
        
        public class Baker : Baker<PlayerTextPrefabAuthoring>
        {
            public override void Bake(PlayerTextPrefabAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                if(entity != Entity.Null)
                {
                    AddComponent(entity, new TMPTextPrefab
                    {
                        EntityPrefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic)
                    });
                }
            }
        }
    }
    
    public struct TMPTextPrefab : IComponentData
    {
        public Entity EntityPrefab;
    }
}