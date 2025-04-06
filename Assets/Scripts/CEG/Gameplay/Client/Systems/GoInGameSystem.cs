using CEG.Gameplay.Shared.Components;
//using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace CEG.Gameplay.Client.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct GoInGameSystem : ISystem, ISystemStartStop
    {
        //[BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            Log.Verbose("OnCreate");
            var builder = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<NetworkId>()
                .WithNone<NetworkStreamInGame>();
            
            state.RequireForUpdate(state.GetEntityQuery(builder));
            
            builder.Dispose();
        }

        //[BurstCompile]
        public void OnStartRunning(ref SystemState state)
        {
            Log.Verbose("OnStartRunning");
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (networkId, entity) in SystemAPI.Query<RefRO<NetworkId>>()
                         .WithNone<NetworkStreamInGame>()
                         .WithEntityAccess())
            {
                Log.Info($"Going In Game with {entity} | NetworkId: {networkId.ValueRO.Value}");

                ecb.AddComponent<ConnectionState>(entity);
                ecb.AddComponent<GhostOwnerIsLocal>(entity);
                ecb.AddComponent<NetworkSnapshotAck>(entity);
                ecb.AddComponent<NetworkStreamInGame>(entity);

                var requestEntity = ecb.CreateEntity();
                ecb.AddComponent<RpcStartStreaming>(requestEntity);
                ecb.AddComponent(requestEntity, new SendRpcCommandRequest { TargetConnection = entity });
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        //[BurstCompile]
        public void OnStopRunning(ref SystemState state)
        {
            Log.Verbose("OnStopRunning");
        }
        
        //[BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            Log.Verbose("OnDestroy");
        }
    }
}