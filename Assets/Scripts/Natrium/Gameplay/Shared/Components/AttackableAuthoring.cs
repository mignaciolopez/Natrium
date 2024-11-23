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
                    AddComponent<Attack>(e);
                    SetComponentEnabled<Attack>(e, false);
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
    public struct Attack : IComponentData, IEnableableComponent
    {
        public Entity SourceServerEntity; //Internal Server Use Only
    }

    public struct RPCAttack : IRpcCommand
    {
        public int NetworkIdSource;
        public int NetworkIdTarget;
    }
}
