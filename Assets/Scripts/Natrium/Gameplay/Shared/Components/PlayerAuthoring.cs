using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.NetCode;
using Unity.Mathematics;

namespace Natrium.Gameplay.Shared.Components
{
    public enum MovementTypeEnum
    {
        Free = 0,
        Diagonal,
        Classic
    }

    [DisallowMultipleComponent]
    public class PlayerAuthoring : MonoBehaviour
    {
        public float speed = 2.0f;
        public MovementTypeEnum movementType = MovementTypeEnum.Classic;
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
            AddComponent(entity, new MovementType
            {
                Value = authoring.movementType
            });
            AddComponent<PlayerPosition>(entity);
            AddComponent<MovementFree>(entity);
            AddComponent<MovementDiagonal>(entity);
            AddComponent<MovementClassic>(entity);

            AddComponent<PlayerInput>(entity);
            AddComponent<Health>(entity);
            AddComponent<MaxHealth>(entity);
            AddComponent<DamagePoints>(entity);
            AddComponent<DebugColor>(entity);
        }
    }

    public struct Player : IComponentData
    {
        [GhostField]
        public FixedString64Bytes Name;
    }

    public struct PlayerPosition : IComponentData
    {
        [GhostField]
        public float3 Previous;

        [GhostField]
        public float3 Next;
    }

    public struct Speed : IComponentData
    {
        [GhostField(Quantization = 100)]
        public float Value;
    }

    public struct Health : IComponentData
    {
        [GhostField]
        public int Value;
    }
    
    public struct MaxHealth : IComponentData
    {
        [GhostField]
        public int Value;
    }
    
    public struct DamagePoints : IComponentData
    {
        [GhostField(Quantization = 100)]
        public float Value;
    }

    public struct DebugColor : IComponentData
    {
        [GhostField(Quantization = 100)]
        public float3 Value;
    }

    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
    public struct PlayerInput : IInputComponentData
    {
        [GhostField(Quantization = 100)]
        public float3 InputAxis;
    }

    public struct MovementType : IComponentData
    {
        [GhostField]
        public MovementTypeEnum Value;
    }

    public struct MovementFree : IComponentData, IEnableableComponent { }
    public struct MovementDiagonal : IComponentData, IEnableableComponent { }
    public struct MovementClassic : IComponentData, IEnableableComponent { }
}
