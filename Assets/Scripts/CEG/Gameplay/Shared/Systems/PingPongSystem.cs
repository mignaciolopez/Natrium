using CEG.Gameplay.Shared.Components;
using System;
using Unity.Entities;
using Unity.NetCode;

namespace CEG.Gameplay.Shared.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial class PingPongSystem : SystemBase
    {
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
        }

        protected override void OnStopRunning()
        {
            Log.Verbose($"[{World.Name}] OnStopRunning");
            base.OnStopRunning();
        }
        
        protected override void OnDestroy()
        {
            Log.Verbose($"[{World.Name}] OnDestroy");
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(WorldUpdateAllocator);
            
            foreach (var entity in SystemAPI.Query<RefRO<PingRequest>>()
                         .WithEntityAccess())
            {
                OnSendPing(ref ecb);
                ecb.DestroyEntity(entity.Item2);
            }
            
            ProcessPings(ref ecb);
            ProcessPongs(ref ecb);

            ecb.Playback(EntityManager);
        }

        private void ProcessPings(ref EntityCommandBuffer ecb)
        {
            foreach (var (rpcPing, receiveRpc, reqEntity) in SystemAPI.Query<RpcPing, ReceiveRpcCommandRequest>().WithEntityAccess())
            {
                SendPong(receiveRpc.SourceConnection, rpcPing.UnixTime, ref ecb);
                ecb.DestroyEntity(reqEntity);
            }
        }

        private void ProcessPongs(ref EntityCommandBuffer ecb)
        {
            foreach (var (rpcPong, receiveRpc, reqEntity) in SystemAPI.Query<RefRW<RpcPong>, ReceiveRpcCommandRequest>().WithEntityAccess())
            {
                SetLatency(receiveRpc.SourceConnection, rpcPong.ValueRO.PongUnixTime, ref ecb);
                SetRTT(receiveRpc.SourceConnection, rpcPong.ValueRO.PingUnixTime, ref ecb);

                ecb.DestroyEntity(reqEntity);
            }
        }

        private void OnSendPing(ref EntityCommandBuffer ecb)
        {
            var unixTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var reqE = ecb.CreateEntity();
            ecb.AddComponent(reqE, new RpcPing
            {
                UnixTime = unixTime
            });
            ecb.AddComponent<SendRpcCommandRequest>(reqE); //Server will broadcast to all clients

            Log.Debug($"[{World.Name}] Ping sent: {unixTime}");
        }

        private void SendPong(Entity sender, long pingUnixTime, ref EntityCommandBuffer ecb)
        {
            var reqE = ecb.CreateEntity();
            ecb.AddComponent(reqE, new RpcPong
            {
                PingUnixTime = pingUnixTime,
                PongUnixTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            });
            ecb.AddComponent(reqE, new SendRpcCommandRequest
            {
                TargetConnection = sender
            });

            Log.Debug($"[{World.Name}] Pong Answered to {sender}");
        }

        private void SetLatency(Entity e, long pongUnixTime, ref EntityCommandBuffer ecb)
        {
            var unixTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var lat = (unixTime - pongUnixTime);

            if (SystemAPI.TryGetSingleton<Latency>(out var latency))
                latency.Value = lat;
            else
                ecb.AddComponent(e, new Latency { Value = lat });

            Log.Info($"[{World.Name}] Latency: {lat} ms");
        }

        private void SetRTT(Entity e, long pingUnixTime, ref EntityCommandBuffer ecb)
        {
            var unixTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var rtt = (unixTime - pingUnixTime);

            if (SystemAPI.TryGetSingleton<RoundTripTime>(out var roundTripTime))
                roundTripTime.Value = rtt;
            else
                ecb.AddComponent(e, new RoundTripTime { Value = rtt });

            Log.Info($"[{World.Name}] RoundTripTime: {rtt} ms");
        }
    }
}
