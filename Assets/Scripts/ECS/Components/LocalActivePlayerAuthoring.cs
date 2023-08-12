using UnityEngine;
using Unity.Entities;
using Unity.NetCode;
using UnityEditor;
using Unity.Mathematics;

namespace Natrium
{
    public enum MovementType
    {
        Free = 0,
        Full_Tile,
        Full_Tile_NoDiagonal
    }

    [DisallowMultipleComponent]
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

    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
    public struct LocalActivePlayerData : IInputComponentData
    {
        public MovementType movementType;
        public float3 InputAxis;
    }

    public struct SpeedData : IComponentData
    {
        public float value;
    }
}
