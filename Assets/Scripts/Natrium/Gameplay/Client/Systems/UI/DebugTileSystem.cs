using Natrium.Gameplay.Client.Components.UI;
using Natrium.Gameplay.Shared.Components;
using Natrium.Gameplay.Shared.Utilities;
using Natrium.Shared;
using Unity.Entities;
using UnityEngine;
using Unity.NetCode;
using Unity.Mathematics;
using Unity.Transforms;

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
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnCreate()");
            base.OnCreate();
        }

        protected override void OnStartRunning()
        {
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnStartRunning()");
            base.OnStartRunning();

            _debugTilePrefab = GameObject.FindAnyObjectByType<DebugTilePrefabAuthoring>().Prefab;
            _debugTile = GameObject.Instantiate(_debugTilePrefab);
            _debugTile.SetActive(false);
        }

        protected override void OnStopRunning()
        {
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnStopRunning()");
            base.OnStopRunning();

            var ecbs = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            _ecb = ecbs.CreateCommandBuffer(World.Unmanaged);

            if (_debugTile != null)
                GameObject.Destroy(_debugTile);
        }

        protected override void OnDestroy()
        {
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnDestroy()");
            base.OnDestroy();
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
            /*foreach(var (rpcTile, rpcEntity) in SystemAPI.Query<RefRO<RpcTile>>().WithAll<ReceiveRpcCommandRequest>().WithNone<RpcTileDrawnTag>().WithEntityAccess())
            {
                _debugTile.SetActive(true);
                _debugTile.transform.position = math.round(rpcTile.ValueRO.End); ;
                _ecb.AddComponent<RpcTileDrawnTag>(rpcEntity);

                //TODO: UI Should Not consume the rpc, just removing the warning cause no one is consuming it rn
                _ecb.DestroyEntity(rpcEntity);
            }*/
        }

        private void DrawDebugAttacks()
        {
            foreach (var (dc, e) in SystemAPI.Query<DebugColor>().WithAll<DebugTileTag>().WithEntityAccess())
            {
                var childs = SystemAPI.GetBuffer<LinkedEntityGroup>(e);
                foreach (var child in childs)
                {
                    if (EntityManager.HasComponent<SpriteRenderer>(child.Value))
                    {
                        EntityManager.GetComponentObject<SpriteRenderer>(child.Value).color = new Color(dc.Value.x, dc.Value.y, dc.Value.z);
                    }
                }
            }
        }
    }
}
