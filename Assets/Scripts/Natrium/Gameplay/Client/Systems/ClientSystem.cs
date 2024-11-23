using Natrium.Shared;
using Natrium.Shared.Systems;
using Unity.Entities;
using Unity.NetCode;
using Natrium.Gameplay.Shared.Components;
using Natrium.Gameplay.Shared;
using System.Net;
using Unity.Networking.Transport;
using System;
using System.Net.Sockets;

namespace Natrium.Gameplay.Client.Systems
{
    [UpdateInGroup(typeof(GameplaySystemGroup))]
    [CreateAfter(typeof(SharedSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial class ClientSystem : SystemBase
    {
        protected override void OnCreate()
        {
            Log.Verbose("OnCreate");
            base.OnCreate();
            RequireForUpdate<SystemsSettings>();
        }

        protected override void OnStartRunning()
        {
            Log.Verbose("OnStartRunning");
            base.OnStartRunning();
            EventSystem.Subscribe(Events.OnKeyCodeReturn, OnKeyCodeReturn);
            EventSystem.Subscribe(Events.OnKeyCodeEscape, OnKeyCodeEscape);
        }

        protected override void OnStopRunning()
        {
            Log.Verbose("OnStopRunning");
            base.OnStopRunning();
            EventSystem.UnSubscribe(Events.OnKeyCodeReturn, OnKeyCodeReturn);
            EventSystem.UnSubscribe(Events.OnKeyCodeEscape, OnKeyCodeEscape);
        }

        protected override void OnDestroy()
        {
            Log.Verbose("OnDestroy");
            base.OnDestroy();
        }
        
        protected override void OnUpdate()
        {
            OnConnect(null);
        }

        private void OnKeyCodeReturn(Stream stream)
        {
            foreach(var conState in SystemAPI.Query<ConnectionState>().WithAll<GhostOwnerIsLocal>())
            {
                Log.Warning($"Connection State: {conState.CurrentState}");
                if (conState.CurrentState == ConnectionState.State.Connecting || conState.CurrentState == ConnectionState.State.Connected || conState.CurrentState == ConnectionState.State.Unknown)
                    return;
            }
            
            if (SystemAPI.TryGetSingleton<SystemsSettings>(out var ss))
            {
                Log.Debug($"SystemsSettings: {ss.Fqdn}:{ss.Port}");

                var ecb = new EntityCommandBuffer(WorldUpdateAllocator);

                IPAddress[] ipv4Addresses = Array.FindAll(Dns.GetHostEntry(ss.Fqdn.ToString()).AddressList, a => a.AddressFamily == AddressFamily.InterNetwork);
                if (ipv4Addresses.Length > 0)
                {
                    Log.Debug($"ipv4Addresses[0]: {ipv4Addresses[0]}");

                    var endpoint = NetworkEndpoint.Parse(ipv4Addresses[0].ToString(), ss.Port);
                    Log.Debug($"endpoint (Dns.GetHostEntry): {endpoint}");

                    var req = ecb.CreateEntity();
                    ecb.AddComponent(req, new NetworkStreamRequestConnect
                    {
                        Endpoint = endpoint
                    });

                    ecb.Playback(EntityManager);
                }
                else
                {
                    Log.Fatal($"Dns.GetHostEntry could not resolve name {ss.Fqdn} to any valid ipv4");
                }
            }
            else
            {
                Log.Fatal($"SystemsSettings Singleton not present!!!");
            }
        }

        private void OnKeyCodeEscape(Stream stream)
        {
            var ecb = new EntityCommandBuffer(WorldUpdateAllocator);

            foreach (var (nid, e) in SystemAPI.Query<NetworkId>().WithEntityAccess().WithAll<NetworkStreamInGame, GhostOwnerIsLocal>())
            {
                Log.Info($"Disconnecting... Entity: {e}, NetworkId: {nid.Value}");
                var req = ecb.CreateEntity();
                ecb.AddComponent<NetworkStreamRequestDisconnect>(e);
            }

            ecb.Playback(EntityManager);
        }

        private void OnConnect(Stream stream)
        {
            var ecb = new EntityCommandBuffer(WorldUpdateAllocator);

            foreach (var (nId, e) in SystemAPI.Query<NetworkId>().WithEntityAccess().WithNone<NetworkStreamInGame>())
            {
                Log.Info($"Connecting... Found Entity: {e} NetworkId: {nId.Value}");

                ecb.AddComponent<NetworkStreamInGame>(e);
                ecb.AddComponent<GhostOwnerIsLocal>(e);
                ecb.AddComponent<ConnectionState>(e);

                var req = ecb.CreateEntity();
                ecb.AddComponent<RpcConnect>(req);
                ecb.AddComponent(req, new SendRpcCommandRequest { TargetConnection = e });
            }

            ecb.Playback(EntityManager);
        }

        private void OnDisconnect(Stream stream)
        {
            throw new NotImplementedException("OnDisconnected");
        }
    }
}
