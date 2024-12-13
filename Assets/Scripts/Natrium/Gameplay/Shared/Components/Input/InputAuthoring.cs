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
                AddComponent<MoveCommand>(entity);
                AddComponent<InputAim>(entity);
                AddComponent<InputMelee>(entity);
            }
        }
    }
    
    public struct InputMove : IComponentData
    { 
        public float2 Value;
    }
    
    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
    public struct MoveCommand : ICommandData
    {
        public NetworkTick Tick { get; set; }
        public float3 Target;
    }

    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
    public struct InputAim : IInputComponentData
    {
        public NetworkTick ServerTick { get; set; }
        public InputEvent InputEvent;
        public float3 MouseWorldPosition;
    }

    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
    public struct InputMelee : IInputComponentData
    {
        public NetworkTick ServerTick { get; set; }
        public InputEvent InputEvent;
    }
}