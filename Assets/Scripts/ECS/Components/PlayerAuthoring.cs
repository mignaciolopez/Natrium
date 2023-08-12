using UnityEngine;
using Unity.Entities;
using Unity.NetCode;
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
    public class PlayerAuthoring : MonoBehaviour
    {
        public MovementType movementType = MovementType.Free;
        public float speed = 2.0f;
    }

    public class PlayerAuthoringBaker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            Entity e = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(e, new PlayerData
            {
                
            });
            AddComponent(e, new SpeedData
            {
                value = authoring.speed
            });
            AddComponent(e, new PlayerInputData
            {
                movementType = authoring.movementType
            });
        }
    }

    public struct PlayerData : IComponentData
    {
        
    }

    public struct SpeedData : IComponentData
    {
        public float value;
    }

    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
    public struct PlayerInputData : IInputComponentData
    {
        public MovementType movementType;
        public float3 InputAxis;
    }
}
