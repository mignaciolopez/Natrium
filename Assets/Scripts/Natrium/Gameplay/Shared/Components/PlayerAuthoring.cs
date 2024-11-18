using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.NetCode;
using Unity.Mathematics;

namespace Natrium.Gameplay.Shared.Components
{
    [DisallowMultipleComponent]
    public class PlayerAuthoring : MonoBehaviour
    {
    }

    public class PlayerBaker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent<PlayerName>(e);
            AddComponent<DebugColor>(e);
            AddComponent<MeeleInput>(e);
            AddComponent<AimInput>(e);
        }
    }

    [GhostComponent(PrefabType = GhostPrefabType.All, OwnerSendType = SendToOwnerType.All)]
    public struct PlayerName : IComponentData
    {
        [GhostField]
        public FixedString64Bytes Value;
    }

    [GhostComponent(PrefabType = GhostPrefabType.All, OwnerSendType = SendToOwnerType.All)]
    public struct DebugColor : IComponentData
    {
        [GhostField(Quantization = 100)]
        public float3 Value;
    }

    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted, OwnerSendType = SendToOwnerType.None)]
    public struct MeeleInput : IInputComponentData
    {
        [GhostField] public InputEvent Input;
    }

    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted, OwnerSendType = SendToOwnerType.SendToNonOwner)]
    public struct AimInput : IInputComponentData
    {
        [GhostField] public InputEvent AimInputEvent;
        [GhostField (Quantization = 0)] public float3 Value; //Mouse World Position
    }
}
