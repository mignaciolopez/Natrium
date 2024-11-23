using Natrium.Gameplay.Shared.Components;
using Natrium.Shared;
using Natrium.Shared.Systems;
using System;
using Unity.Entities;
using Unity.NetCode;

namespace Natrium.Gameplay.Shared.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial class PingPongSystem : SystemBase
    {
        private EntityCommandBuffer _ecb;
        protected override void OnCreate()
        {
            Log.Verbose($"[{World.Name}] OnCreate");
            base.OnCreate();

            RequireForUpdate<SystemsSettings>();
        }

        protected override void OnStartRunning()
        {
            Log.Verbose($"[{World.Name}] OnStartRunning");
            base.OnStartRunning();
            EventSystem.Subscribe(Events.OnSendPing, OnSendPing);
        }

        protected override void OnStopRunning()
        {
            Log.Verbose($"[{World.Name}] OnStopRunning");
            base.OnStopRunning();
            EventSystem.UnSubscribe(Events.OnSendPing, OnSendPing);
        }
        
        protected override void OnDestroy()
        {
            Log.Verbose($"[{World.Name}] OnDestroy");
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            _ecb = new EntityCommandBuffer(WorldUpdateAllocator);

            ProcessPings();
            ProcessPongs();

            _ecb.Playback(EntityManager);
        }

        private void ProcessPings()
        {
            foreach (var (rpcPing, receiveRpc, reqEntity) in SystemAPI.Query<RpcPing, ReceiveRpcCommandRequest>().WithEntityAccess())
            {
                SendPong(receiveRpc.SourceConnection, rpcPing.UnixTime);
                _ecb.DestroyEntity(reqEntity);
            }
        }

        private void ProcessPongs()
        {
            foreach (var (rpcPong, receiveRpc, reqEntity) in SystemAPI.Query<RefRW<RpcPong>, ReceiveRpcCommandRequest>().WithEntityAccess())
            {
                SetLatency(receiveRpc.SourceConnection, rpcPong.ValueRO.PongUnixTime);
                SetRTT(receiveRpc.SourceConnection, rpcPong.ValueRO.PingUnixTime);

                _ecb.DestroyEntity(reqEntity);
            }
        }

        private void OnSendPing(Stream stream)
        {
            var unixTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var ecb = new EntityCommandBuffer(WorldUpdateAllocator);

            var reqE = ecb.CreateEntity();
            ecb.AddComponent(reqE, new RpcPing
            {
                UnixTime = unixTime
            });
            ecb.AddComponent<SendRpcCommandRequest>(reqE); //Server will broadcast to all clients

            ecb.Playback(EntityManager);
            ecb.Dispose();

            Log.Debug($"[{World.Name}] Ping sent: {unixTime}");
        }

        private void SendPong(Entity sender, long pingUnixTime)
        {
            var reqE = _ecb.CreateEntity();
            _ecb.AddComponent(reqE, new RpcPong
            {
                PingUnixTime = pingUnixTime,
                PongUnixTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            });
            _ecb.AddComponent(reqE, new SendRpcCommandRequest
            {
                TargetConnection = sender
            });

            Log.Debug($"[{World.Name}] Pong Answered to {sender}");
        }

        private void SetLatency(Entity e, long pongUnixTime)
        {
            var unixTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var lat = (unixTime - pongUnixTime);

            if (SystemAPI.TryGetSingleton<Latency>(out var latency))
                latency.Value = lat;
            else
                _ecb.AddComponent(e, new Latency { Value = lat });

            Log.Info($"[{World.Name}] Latency: {lat} ms");
        }

        private void SetRTT(Entity e, long pingUnixTime)
        {
            var unixTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var rtt = (unixTime - pingUnixTime);

            if (SystemAPI.TryGetSingleton<RoundTripTime>(out var roundTripTime))
                roundTripTime.Value = rtt;
            else
                _ecb.AddComponent(e, new RoundTripTime { Value = rtt });

            Log.Info($"[{World.Name}] RoundTripTime: {rtt} ms");
        }
    }
}
