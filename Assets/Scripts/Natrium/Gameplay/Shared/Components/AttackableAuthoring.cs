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
                var e = GetEntity(TransformUsageFlags.Dynamic);
                if (e != Entity.Null)
                {
                    AddComponent<AttackEvents>(e);
                    AddComponent<AttackableTag>(e);
                    AddBuffer<DamagePointsBuffer>(e);
                    AddBuffer<DamagePointsAtTick>(e);
                    AddComponent<Team>(e);
                }
            }
        }
    }
    
    [GhostComponent(PrefabType = GhostPrefabType.All)]
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

    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted, OwnerSendType = SendToOwnerType.All)]
    public struct Team : IComponentData
    {
        [GhostField] public TeamEnum Value;
    }
    
    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted, OwnerSendType = SendToOwnerType.All)]
    public struct AttackEvents : IBufferElementData
    {
        [GhostField] public NetworkTick Tick;
        [GhostField] public Entity EntitySource;
        [GhostField] public Entity EntityTarget;
        [GhostField] public int NetworkIdSource;
        [GhostField] public int NetworkIdTarget;
        [GhostField] public int LifeTime;
    }
}
