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
    
    [GhostComponent(PrefabType = GhostPrefabType.All, OwnerSendType = SendToOwnerType.All)]
    public struct AttacksBuffer : IBufferElementData
    {
        [GhostField] public NetworkTick ServerTick { get; set; }
        [GhostField] public NetworkTick InterpolationTick { get; set; }
        [GhostField] public Entity EntitySource;
        [GhostField] public int NetworkIdSource;
    }
}
