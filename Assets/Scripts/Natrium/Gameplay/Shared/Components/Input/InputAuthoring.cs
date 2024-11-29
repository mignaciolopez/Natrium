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
                AddComponent<InputAim>(e);
                AddComponent<InputMelee>(e);
            }
        }
    }
    
    public struct InputMove : IInputComponentData
    {
        public float2 Value;
    }

    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
    public struct InputAim : ICommandData
    {
        public NetworkTick Tick { get; set; }
        public bool Set;
        public float3 MouseWorldPosition;
    }
    
    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
    public struct InputMelee : ICommandData
    {
        public NetworkTick Tick { get; set; }
        public bool Set;
    }
    
    //IInputComponentData vs ICommandData
    //Both needs to set a bool or an InputEvent to false on every frame.
    //IInputComponentData set InputEvent = default.
    //ICommandData set a bool to false, also can use InputEvent.
    //When the event takes place in the gameplay we set to true the event/bool
    //and we set the network tick
    //They are basically the same shit. I see no Difference.
    //OwnerSendType = SendToOwnerType.SendToNonOwner) has no effect.
    //It does not replicate to all clients.
}