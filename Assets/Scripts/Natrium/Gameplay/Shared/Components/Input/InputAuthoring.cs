using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

namespace Natrium.Gameplay.Shared.Components.Input
{
    public class InputAuthoring : MonoBehaviour
    {


        public class Baker : Baker<InputAuthoring>
        {
            public override void Bake(InputAuthoring authoring)
            {
                var e = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<InputMove>(e);
                AddComponent<InputMeele>(e);
                AddComponent<InputAim>(e);
            }
        }
    }

    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted, OwnerSendType = SendToOwnerType.None)]
    public struct InputMove : IInputComponentData
    {
        [GhostField(Quantization = 0)]
        public float2 InputAxis;
    }
    
    public struct InputMeele : IInputComponentData
    {
        [GhostField] public InputEvent InputEvent;
    }

    public struct InputAim : IInputComponentData
    {
        [GhostField] public InputEvent InputEvent;
        [GhostField(Quantization = 0)] public float3 MouseWorldPosition;
    }
}