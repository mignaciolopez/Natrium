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
    public class SystemSettingsAuthoring : MonoBehaviour
    {
        [Header("Movement System")]
        public MovementType movementType = MovementType.Free;
    }

    public class SystemSettingsBaker : Baker<SystemSettingsAuthoring>
    {
        public override void Bake(SystemSettingsAuthoring authoring)
        {
            Entity e = GetEntity(TransformUsageFlags.None);

            AddComponent(e, new SystemSettingsData
            {
                movementType = authoring.movementType
            });
        }
    }

    public struct SystemSettingsData : IComponentData
    {
        public MovementType movementType;
    }
}
