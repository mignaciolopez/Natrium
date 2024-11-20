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
    }

    public class AttackableBaker : Baker<AttackableAuthoring>
    {
        public override void Bake(AttackableAuthoring authoring)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);
            if (e != Entity.Null)
            {
                AddComponent<Attack>(e);
                SetComponentEnabled<Attack>(e, false);
                AddComponent<AttackableTag>(e);
                AddComponent<CurrentHealthPoints>(e);
                AddComponent<MaxHealthPoints>(e);
                AddBuffer<DamagePointsBuffer>(e);
                AddBuffer<DamagePointsAtTick>(e);
                AddComponent<CitizenShip>(e);
            }
        }
    }

    [GhostComponent(PrefabType = GhostPrefabType.All)]
    public struct AttackableTag : IComponentData { }

    [GhostComponent(PrefabType = GhostPrefabType.All, OwnerSendType = SendToOwnerType.SendToOwner)]
    public struct CurrentHealthPoints : IComponentData
    {
        [GhostField] public int Value;
    }

    [GhostComponent(PrefabType = GhostPrefabType.All, OwnerSendType = SendToOwnerType.SendToOwner)]
    public struct MaxHealthPoints : IComponentData
    {
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
