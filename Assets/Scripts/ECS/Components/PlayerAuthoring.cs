using UnityEngine;
using Unity.Entities;
using Unity.NetCode;
using Unity.Mathematics;

namespace Natrium
{
    [DisallowMultipleComponent]
    public class PlayerAuthoring : MonoBehaviour
    {
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
        public float value;
    }

    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
    public struct PlayerInputData : IInputComponentData
    {
        public float3 InputAxis;
    }
}
