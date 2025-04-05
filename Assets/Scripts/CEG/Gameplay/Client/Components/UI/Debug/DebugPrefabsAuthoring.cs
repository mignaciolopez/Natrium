using UnityEngine;
using Unity.Entities;

namespace CEG.Gameplay.Client.Components.UI.Debug
{
    [DisallowMultipleComponent]
    public class DebugPrefabsAuthoring : MonoBehaviour
    {
        public GameObject aimInputPrefab;
        public GameObject damagePrefab;
        
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
                    
                    AddComponent(e, new DamagePrefab
                    {
                        Prefab = GetEntity(authoring.damagePrefab, TransformUsageFlags.Dynamic)
                    });
                }
            }
        }
    }

    public struct DebugAimInputPrefab : IComponentData
    {
        public Entity Prefab;
    }
    
    public struct DamagePrefab : IComponentData
    {
        public Entity Prefab;
    }
}
