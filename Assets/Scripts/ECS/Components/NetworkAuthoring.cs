using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace Natrium
{
    public class NetworkAuthoring : MonoBehaviour
    {

    }

    public class NetworkBaker : Baker<NetworkAuthoring>
    {
        public override void Bake(NetworkAuthoring authoring)
        {
            Entity e = GetEntity(TransformUsageFlags.None);
            AddComponent(e, new NetworkData
            {
                
            });
        }
    }

    public struct NetworkData : IComponentData
    {
        
    }

    public struct Rpc_Connect : IRpcCommand { }
    public struct Rpc_Disconnect : IRpcCommand { }
    public struct Rpc_Click : IRpcCommand { }
    public struct Rpc_Spawn : IRpcCommand { }
}