using Natrium.Gameplay.Client.Components.UI;
using Natrium.Gameplay.Shared.Components;
using Natrium.Gameplay.Shared.Utilities;
using Unity.Entities;
using UnityEngine;
using Unity.NetCode;
using Unity.Mathematics;

namespace Natrium.Gameplay.Client.Systems.UI
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class DebugTileSystem : SystemBase
    {
        private GameObject _debugTilePrefab;
        private GameObject _debugTile;

        private EntityCommandBuffer _ecb;
        protected override void OnCreate()
        {
            base.OnCreate();

            RequireForUpdate<DebugTileSystemExecute>();
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();

            _debugTilePrefab = GameObject.FindAnyObjectByType<DebugTilePrefabAuthoring>().Prefab;
            _debugTile = GameObject.Instantiate(_debugTilePrefab);
            _debugTile.SetActive(false);
        }

        protected override void OnStopRunning()
        {
            base.OnStopRunning();

            var ecbs = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            _ecb = ecbs.CreateCommandBuffer(World.Unmanaged);

            if (_debugTile != null)
                GameObject.Destroy(_debugTile);
        }

        protected override void OnUpdate()
        {
            var ecbs = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            _ecb = ecbs.CreateCommandBuffer(World.Unmanaged);

            DrawDebugTiles();
            DrawDebugAttacks();
        }

        private void DrawDebugTiles()
        {
            foreach(var (rpcTile, rpcEntity) in SystemAPI.Query<RefRO<RpcTile>>().WithAll<ReceiveRpcCommandRequest>().WithNone<RpcTileDrawnTag>().WithEntityAccess())
            {
                _debugTile.SetActive(true);
                _debugTile.transform.position = math.round(rpcTile.ValueRO.End);
                _ecb.AddComponent<RpcTileDrawnTag>(rpcEntity);

                //TODO: UI Should Not consume the rpc, just removing the warning cause no one is consuming it rn
                _ecb.DestroyEntity(rpcEntity);
            }
        }

        private void DrawDebugAttacks()
        {
            foreach (var (rpcA, rpcEntity) in SystemAPI.Query<RefRO<RpcAttack>>().WithAll<ReceiveRpcCommandRequest>().WithNone<RpcTileDrawnTag>().WithEntityAccess())
            {
                var entitySource = Utils.GetEntityPrefab(rpcA.ValueRO.NetworkIdSource, EntityManager);
                var DebugColor = EntityManager.GetComponentData<DebugColor>(entitySource);

                var offset = new float3(0, 1.6f, 0);
                var gameObject = GameObject.Instantiate(_debugTilePrefab, math.round(rpcA.ValueRO.End + offset), Quaternion.identity);
                gameObject.GetComponentInChildren<SpriteRenderer>().color = new Color(DebugColor.Value.x, DebugColor.Value.y, DebugColor.Value.z);
                GameObject.Destroy(gameObject, 1.0f);

                //TODO: UI Should Not consume the rpc, just removing the warning cause no one is consuming it rn
                _ecb.DestroyEntity(rpcEntity);
            }
        }
    }
}
