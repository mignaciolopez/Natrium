using UnityEngine;
using Unity.Entities;

namespace Natrium.ECS.Components
{
    public enum MovementType
    {
        Free = 0,
        FullTile,
        FullTileNoDiagonal
    }

    public class LocalActivePlayerAuthoring : MonoBehaviour
    {
        public MovementType movementType = MovementType.Free;
        public float speed = 2.0f;
    }

    public class LocalActivePlayerAuthoringBaker : Baker<LocalActivePlayerAuthoring>
    {
        public override void Bake(LocalActivePlayerAuthoring authoring)
        {
            Entity entity = this.GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new LocalActivePlayerData
            {
                movementType = authoring.movementType
            });
            AddComponent(entity, new SpeedData
            {
                value = authoring.speed
            });
        }
    }

    public struct LocalActivePlayerData : IComponentData
    {
        public MovementType movementType;
    }

    public struct SpeedData : IComponentData
    {
        public float value;
    }
}
