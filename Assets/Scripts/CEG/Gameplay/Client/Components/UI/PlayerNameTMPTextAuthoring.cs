using System;
using TMPro;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace CEG.Gameplay.Client.Components.UI
{
    public class PlayerNameTMPTextAuthoring : MonoBehaviour
    {
        public class Baker : Baker<PlayerNameTMPTextAuthoring>
        {
            public override void Bake(PlayerNameTMPTextAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponentObject(entity, new PlayerNameTMPText());
                AddComponent<PlayerNameTMPTextProperties>(entity);
            }
        }
    }
    
    public class PlayerNameTMPText : IComponentData, IDisposable
    {
        public GameObject GameObject;
        public TMP_Text Value;
        
        public void Dispose()
        {
            if (GameObject != null)
            {
                GameObject.DestroyImmediate(GameObject);
                Value = null;
            }
        }
    }
    
    public struct PlayerNameTMPTextProperties : IComponentData
    {
        public float3 Offset;
    }

    public struct PlayerNameTMPTextInitialized : IComponentData { }
}
