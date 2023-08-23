using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
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
    public struct Rpc_Click : IRpcCommand
    {
        [GhostField (Quantization = 10)] public float3 mousePosition;
    }
    public struct TouchData : IRpcCommand
    {
        public int3 tile;
    }

    public struct HitEntity : IRpcCommand
    {
        public Entity entity;
    }
}