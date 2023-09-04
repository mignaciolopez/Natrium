using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.NetCode;
using Unity.Mathematics;

namespace Natrium.Gameplay.Components
{
    [DisallowMultipleComponent]
    public class PlayerAuthoring : MonoBehaviour
    {
        public float speed = 2.0f;
    }

    public class PlayerBaker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent<Player>(entity);
            AddComponent(entity, new Speed
            {
                Value = authoring.speed
            });
            AddComponent<PlayerInput>(entity);
            AddComponent<Health>(entity);
            AddComponent<MaxHealth>(entity);
            AddComponent<DamagePoints>(entity);
        }
    }

    public struct Player : IComponentData
    {
        public FixedString64Bytes Name;
        public int3 PreviousPos;
        public int3 NextPos;
    }

    public struct Speed : IComponentData
    {
        [GhostField(Quantization = 100)]
        public float Value;
    }

    public struct Health : IComponentData
    {
        public int Value;
    }
    
    public struct MaxHealth : IComponentData
    {
        public int Value;
    }
    
    public struct DamagePoints : IComponentData
    {
        [GhostField(Quantization = 100)]
        public float Value;
    }
    
    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
    public struct PlayerInput : IInputComponentData
    {
        [GhostField(Quantization = 100)]
        public float3 InputAxis;
    }
}
