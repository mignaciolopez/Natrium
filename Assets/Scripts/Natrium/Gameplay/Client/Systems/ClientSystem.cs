using Unity.Entities;
using Natrium.Shared.Systems;
using Natrium.Shared;
using Unity.Collections;
using Unity.NetCode;
using Natrium.Gameplay.Shared.Components;
using Natrium.Gameplay.Shared;
using System.Net;
using Unity.Networking.Transport;
using Natrium.Gameplay.Client.Components;
using System;

namespace Natrium.Gameplay.Client.Systems
{
    [UpdateInGroup(typeof(GameplaySystemGroup))]
    [CreateAfter(typeof(SharedSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial class ClientSystem : SystemBase
    {
        protected override void OnCreate()
        {
            base.OnCreate();

            RequireForUpdate<ClientSystemExecute>();
            RequireForUpdate<SystemsSettings>();
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            EventSystem.Subscribe(Events.OnConnected, OnConnected);
            EventSystem.Subscribe(Events.OnDisconnected, OnDisconnected);
            EventSystem.Subscribe(Events.OnKeyCodeReturn, OnKeyCodeReturn);
            EventSystem.Subscribe(Events.OnKeyCodeEscape, OnKeyCodeEscape);
        }

        protected override void OnStopRunning()
        {
            base.OnStopRunning();
            EventSystem.UnSubscribe(Events.OnConnected, OnConnected);
            EventSystem.UnSubscribe(Events.OnDisconnected, OnDisconnected);
            EventSystem.UnSubscribe(Events.OnKeyCodeReturn, OnKeyCodeReturn);
            EventSystem.UnSubscribe(Events.OnKeyCodeEscape, OnKeyCodeEscape);
        }

        protected override void OnUpdate()
        {
            OnConnected(null);
        }

        private void OnKeyCodeReturn(Stream stream)
        {
            var ss = SystemAPI.GetSingleton<SystemsSettings>();

            var serverAddress = IPAddress.Parse(ss.IP.ToString());
            var nativeArrayAddress = new NativeArray<byte>(serverAddress.GetAddressBytes().Length, Allocator.Temp);
            nativeArrayAddress.CopyFrom(serverAddress.GetAddressBytes());

            var endpoint = NetworkEndpoint.AnyIpv4;
            endpoint.SetRawAddressBytes(nativeArrayAddress);
            endpoint.Port = ss.Port;

            using var query = WorldManager.ClientWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            var driver = query.GetSingletonRW<NetworkStreamDriver>().ValueRW;
            driver.Connect(WorldManager.ClientWorld.EntityManager, endpoint);

            nativeArrayAddress.Dispose();
        }

        private void OnKeyCodeEscape(Stream stream)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (nid, e) in SystemAPI.Query<NetworkId>().WithEntityAccess().WithAll<NetworkStreamInGame, GhostOwnerIsLocal>())
            {
                UnityEngine.Debug.Log($"'{World.Unmanaged.Name}' Disconnecting... Entity: {e}, NetworkId: {nid.Value}");
                var req = ecb.CreateEntity();
                ecb.AddComponent<NetworkStreamRequestDisconnect>(e);
            }

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }

        private void OnConnected(Stream stream)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            var found = false;

            foreach (var (nId, e) in SystemAPI.Query<NetworkId>().WithEntityAccess().WithNone<NetworkStreamInGame>())
            {
                found = true;
                UnityEngine.Debug.Log($"'{World.Unmanaged.Name}' Connecting... Found Entity: {e} NetworkId: {nId.Value}");

                ecb.AddComponent<NetworkStreamInGame>(e);
                ecb.AddComponent<GhostOwnerIsLocal>(e);

                var req = ecb.CreateEntity();
                ecb.AddComponent<RpcConnect>(req);
                ecb.AddComponent(req, new SendRpcCommandRequest { TargetConnection = e });
            }

            if (!found)
            {
                //UnityEngine.Debug.LogWarning($"'{World.Unmanaged.Name}' Entity Not Found");
            }

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }

        private void OnDisconnected(Stream stream)
        {
            throw new NotImplementedException("OnDisconnected");
        }
    }
}
