using CEG.Gameplay.Client.Components;
using CEG.Gameplay.Shared;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using System;
using System.Net;
using System.Net.Sockets;

namespace CEG.Gameplay.Client.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
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
        }

        protected override void OnStopRunning()
        {
            Log.Verbose("OnStopRunning");
            base.OnStopRunning();
        }

        protected override void OnDestroy()
        {
            Log.Verbose("OnDestroy");
            base.OnDestroy();
        }
        
        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(WorldUpdateAllocator);

            var shouldConnect = false;
            var shouldDisconnect = false;

            foreach (var entity in SystemAPI.Query<RefRO<ConnectRequest>>()
                         .WithNone<DisconnectRequest>()
                         .WithEntityAccess())
            {
                shouldConnect = true;
                ecb.DestroyEntity(entity.Item2);
            }
            
            foreach (var entity in SystemAPI.Query<RefRO<DisconnectRequest>>().WithEntityAccess())
            {
                shouldConnect = false;
                shouldDisconnect = true;
                ecb.DestroyEntity(entity.Item2);
            }

            ecb.Playback(EntityManager);

            if (shouldConnect)
                Connect();
            if (shouldDisconnect)
                Disconnect();
        }

        private void Connect()
        {
            foreach(var connectionState in SystemAPI.Query<RefRO<ConnectionState>>().WithAll<GhostOwnerIsLocal>())
            {
                if (connectionState.ValueRO.CurrentState
                    is ConnectionState.State.Connecting
                    or ConnectionState.State.Connected
                    or ConnectionState.State.Unknown)
                {
                    Log.Warning($"Connection State: {connectionState.ValueRO.CurrentState}");
                    return;
                }
            }

            foreach (var networkStreamRequestConnect in SystemAPI.Query<RefRO<NetworkStreamRequestConnect>>())
            {
                Log.Info($"Already Connecting to :{networkStreamRequestConnect.ValueRO.Endpoint}");
                return;
            }
            
            if (SystemAPI.TryGetSingleton<SystemsSettings>(out var ss))
            {
                Log.Debug($"SystemsSettings: {ss.Fqdn}:{ss.Port}");

                var ipv4Addresses = Array.FindAll(Dns.GetHostEntry(ss.Fqdn.ToString()).AddressList, a => a.AddressFamily == AddressFamily.InterNetwork);
                if (ipv4Addresses.Length > 0)
                {
                    var endpoint = NetworkEndpoint.Parse(ipv4Addresses[0].ToString(), ss.Port);
                    Log.Info($"Connecting to: {endpoint}");

                    var requestEntity = EntityManager.CreateEntity();
                    EntityManager.AddComponent<NetworkStreamRequestConnect>(requestEntity);
                    EntityManager.SetComponentData(requestEntity, new NetworkStreamRequestConnect
                    {
                        Endpoint = endpoint
                    });
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

        private void Disconnect()
        {
            var ecb = new EntityCommandBuffer(WorldUpdateAllocator);

            foreach (var (networkId, entity) in SystemAPI.Query<RefRO<NetworkId>>()
                         .WithAll<NetworkStreamInGame, GhostOwnerIsLocal>()
                         .WithEntityAccess())
            {
                Log.Info($"Disconnecting... {entity}, NetworkId: {networkId.ValueRO.Value}");
                ecb.AddComponent<NetworkStreamRequestDisconnect>(entity);
            }

            ecb.Playback(EntityManager);
        }
    }
}
