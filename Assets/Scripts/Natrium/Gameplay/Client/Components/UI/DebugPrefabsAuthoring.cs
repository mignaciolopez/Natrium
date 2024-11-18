using UnityEngine;
using Unity.Entities;

namespace Natrium.Gameplay.Client.Components.UI
{
    [DisallowMultipleComponent]
    public class DebugPrefabsAuthoring : MonoBehaviour
    {
        public GameObject aimInputPrefab;
        
        public class Baker : Baker<DebugPrefabsAuthoring>
        {
            public override void Bake(DebugPrefabsAuthoring authoring)
            {
                var e = GetEntity(TransformUsageFlags.Dynamic);
                if(e != Entity.Null)
                {
                    AddComponent(e, new DebugAimInputPrefab
                    {
                        Prefab = GetEntity(authoring.aimInputPrefab, TransformUsageFlags.Dynamic)
                    });
                }
            }
        }
    }

    public struct DebugAimInputPrefab : IComponentData
    {
        public Entity Prefab;
    }
}
