using UnityEngine;
using Unity.Entities;

namespace Natrium
{
    public enum MovementType
    {
        Free = 0,
        Full_Tile,
        Full_Tile_NoDiagonal
    }

    public class LocalActivePlayerAuthoring : MonoBehaviour
    {
        public MovementType movementType = MovementType.Free;
        public float speed = 2.0f;
        public bool autoDistance = false;
        public float minDistanceInput = 0.1f; 
    }

    public class LocalActivePlayerAuthoringBaker : Baker<LocalActivePlayerAuthoring>
    {
        public override void Bake(LocalActivePlayerAuthoring authoring)
        {
            Entity entity = this.GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new LocalActivePlayerData
            {
                movementType = authoring.movementType,
                autoDistance = authoring.autoDistance,
                minDistanceInput = authoring.minDistanceInput
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
        public bool autoDistance;
        public float minDistanceInput;
    }

    public struct SpeedData : IComponentData
    {
        public float value;
    }
}
