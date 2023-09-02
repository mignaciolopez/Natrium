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
            var e = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(e, new PlayerData
            {

            });
            AddComponent(e, new SpeedData
            {
                Value = authoring.speed
            });
            AddComponent(e, new PlayerInputData
            {
                
            });
        }
    }

    public struct PlayerData : IComponentData
    {
        public int3 PreviousPos;
        public int3 NextPos;
    }

    public struct SpeedData : IComponentData
    {
        public float Value;
    }

    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
    public struct PlayerInputData : IInputComponentData
    {
        [GhostField(Quantization = 100)] public float3 InputAxis;
    }
}
