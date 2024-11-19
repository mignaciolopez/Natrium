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
    public class MovementAuthoring : MonoBehaviour
    {
        public float speed = 2.0f;
        public MovementTypeEnum movementType = MovementTypeEnum.Classic;
    }

    public class MovementBaker : Baker<MovementAuthoring>
    {
        public override void Bake(MovementAuthoring authoring)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(e, new Speed
            {
                Value = authoring.speed
            });

            AddComponent(e, new MovementType
            {
                Value = authoring.movementType
            });

            AddComponent<PlayerTilePosition>(e);

            AddComponent<MovementFree>(e);
            AddComponent<MovementDiagonal>(e);
            AddComponent<MovementClassic>(e);

            switch (authoring.movementType)
            {
                case MovementTypeEnum.Free:
                    SetComponentEnabled<MovementFree>(e, true);
                    SetComponentEnabled<MovementDiagonal>(e, false);
                    SetComponentEnabled<MovementClassic>(e, false);
                    break;
                case MovementTypeEnum.Diagonal:
                    SetComponentEnabled<MovementFree>(e, false);
                    SetComponentEnabled<MovementDiagonal>(e, true);
                    SetComponentEnabled<MovementClassic>(e, false);
                    break;
                case MovementTypeEnum.Classic:
                    SetComponentEnabled<MovementFree>(e, false);
                    SetComponentEnabled<MovementDiagonal>(e, false);
                    SetComponentEnabled<MovementClassic>(e, true);
                    break;
            }
        }
    }

    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted, OwnerSendType = SendToOwnerType.SendToOwner)]
    public struct Speed : IComponentData
    {
        [GhostField(Quantization = 100)]
        public float Value;
    }

    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted, OwnerSendType = SendToOwnerType.SendToOwner)]
    public struct PlayerTilePosition : IComponentData
    {
        [GhostField(Quantization = 0)]
        public float3 Previous;

        [GhostField(Quantization = 0)]
        public float3 Target;
    }

    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted, OwnerSendType = SendToOwnerType.SendToOwner)]
    public struct MovementType : IComponentData
    {
        [GhostField]
        public MovementTypeEnum Value;
    }

    public struct MovementFree : IComponentData, IEnableableComponent { }
    public struct MovementDiagonal : IComponentData, IEnableableComponent { }
    public struct MovementClassic : IComponentData, IEnableableComponent { }
}
