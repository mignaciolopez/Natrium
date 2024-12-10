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
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent<InputMove>(entity);
                AddComponent<InputAim>(entity);
                AddComponent<InputMelee>(entity);
            }
        }
    }
    
    public struct InputMove : IComponentData
    { 
        public float2 Value;
    }

    public struct InputAim : ICommandData
    {
        public NetworkTick Tick { get; set; }
        public bool Set;
        public float3 MouseWorldPosition;
    }

    public struct InputMelee : ICommandData
    {
        public NetworkTick Tick { get; set; }
        public bool Set;
    }
}