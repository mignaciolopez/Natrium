using Unity.NetCode;
using UnityEngine;
using Unity.Entities;

namespace Natrium.Gameplay.Shared.Components
{
    public enum CitizenShipEnum
    {
        Neutral = 0,
        Citizen,
        Criminal
    }

    [DisallowMultipleComponent]
    public class AttackableAuthoring : MonoBehaviour
    {
        public int healthPoints;
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
                    AddComponent(e, new HealthPoints
                    {
                        MaxValue = authoring.healthPoints
                    });
                    AddBuffer<DamagePointsBuffer>(e);
                    AddBuffer<DamagePointsAtTick>(e);
                    AddComponent<CitizenShip>(e);
                }
            }
        }
    }
    
    [GhostComponent(PrefabType = GhostPrefabType.All)]
    public struct AttackableTag : IComponentData { }

    [GhostComponent(PrefabType = GhostPrefabType.All, OwnerSendType = SendToOwnerType.All)]
    public struct HealthPoints : IComponentData
    {
        public int MaxValue;
        [GhostField] public int Value;
    }

    [GhostComponent(PrefabType = GhostPrefabType.All)]
    public struct DamagePointsBuffer : IBufferElementData
    {
        public float Value;
    }

    [GhostComponent(PrefabType = GhostPrefabType.All, OwnerSendType = SendToOwnerType.All)]
    public struct DamagePointsAtTick : ICommandData
    {
        public NetworkTick Tick { get; set; }
        public float Value;
    }

    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted, OwnerSendType = SendToOwnerType.All)]
    public struct CitizenShip : IComponentData
    {
        [GhostField] public CitizenShipEnum Value;
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
