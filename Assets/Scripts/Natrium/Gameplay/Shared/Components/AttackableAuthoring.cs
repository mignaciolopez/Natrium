using Unity.NetCode;
using UnityEngine;
using Unity.Entities;

namespace Natrium.Gameplay.Shared.Components
{
    public enum TeamEnum
    {
        Neutral = 0,
        Citizen,
        Criminal
    }

    [DisallowMultipleComponent]
    public class AttackableAuthoring : MonoBehaviour
    {
        public class Baker : Baker<AttackableAuthoring>
        {
            public override void Bake(AttackableAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                if (entity == Entity.Null)
                {
                    return;
                }
                
                AddComponent<AttacksBuffer>(entity);
                AddComponent<AttackableTag>(entity);
                AddBuffer<DamagePointsBuffer>(entity);
                AddBuffer<DamagePointsAtTick>(entity);
                AddComponent<Team>(entity);
            }
        }
    }
    
    public struct AttackableTag : IComponentData { }
    
    public struct DamagePointsBuffer : IBufferElementData
    {
        public float Value;
    }
    
    public struct DamagePointsAtTick : ICommandData
    {
        public NetworkTick Tick { get; set; }
        public float Value;
    }

    [GhostComponent(PrefabType = GhostPrefabType.All, OwnerSendType = SendToOwnerType.All)]
    public struct Team : IComponentData
    {
        [GhostField] public TeamEnum Value;
    }
    
    public struct AttacksBuffer : IBufferElementData //Server Only
    {
        public Entity EntitySource; //Server Only
        public Entity EntityTarget; //Server Only
    }

    public struct RPCAttack : IRpcCommand
    {
        public int NetworkIdSource;
        public int NetworkIdTarget;
    }
}
