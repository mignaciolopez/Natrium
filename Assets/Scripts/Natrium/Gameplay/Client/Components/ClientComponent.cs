using UnityEngine;
using Unity.Entities;
using Unity.Collections;

namespace Natrium.Gameplay.Client.Components
{
    [DisallowMultipleComponent]
    public class ClientAuthoring : MonoBehaviour
    {
        public string IP = "127.0.0.1";
        public ushort Port = 7979;
    }

    public class ClientBaker : Baker<ClientAuthoring>
    {
        public override void Bake(ClientAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new ClientConnectionData
            {
                IP = authoring.IP,
                Port = authoring.Port
            });
        }
    }

    public struct ClientConnectionData : IComponentData
    {
        public FixedString32Bytes IP;
        public ushort Port;
    }
}