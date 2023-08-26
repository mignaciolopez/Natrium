using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.VisualScripting;
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
        public float3 mouseWorldPosition;
    }
    public struct TouchData : IRpcCommand
    {
        public float3 start;
        public float3 end;
    }

    public struct RaycastCommand : IComponentData
    {
        public Entity reqE;
        public float3 Start;
        public float3 End;
        public float MaxDistance;
    }

    public struct RaycastOutput : IComponentData
    {
        public Entity reqE;
        public float3 start;
        public float3 end;
        public Unity.Physics.RaycastHit hit;
    }
}