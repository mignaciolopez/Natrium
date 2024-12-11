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
        public int3 Target;
    }

    [GhostComponent(PrefabType = GhostPrefabType.All, OwnerSendType = SendToOwnerType.SendToNonOwner)]
    [InternalBufferCapacity(512)] //it remains at 64 at runtime
    public struct InputAim : ICommandData
    {
        [GhostField] public NetworkTick Tick { get; set; }
        [GhostField] public bool Set;
        [GhostField] public float3 MouseWorldPosition;
    }

    public struct InputMelee : ICommandData
    {
        public NetworkTick Tick { get; set; }
        public bool Set;
    }
}