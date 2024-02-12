using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using Natrium.Gameplay.Shared.Components;
using Natrium.Gameplay.Shared;
using Natrium.Gameplay.Shared.Utilities;

namespace Natrium.Gameplay.Client.UI
{
    public class UI : MonoBehaviour
    {
        [SerializeField] private GameObject _playerTextPrefab;
        [SerializeField] private GameObject _debugTilePrefab;

        private GameObject _lastDebugTile;

        private Dictionary<Entity, GameObject> _playerNames;

        private RpcTile _lastTile;
        private bool _receivedTile = false;
        private void OnEnable()
        {
            _playerNames = new Dictionary<Entity, GameObject>();
            _lastTile = new RpcTile();

            _lastDebugTile = Instantiate(_debugTilePrefab);
            _lastDebugTile.GetComponentInChildren<SpriteRenderer>().color = Color.gray;
            _lastDebugTile.SetActive(false);
        }
        
        private void OnDisable()
        {
            Destroy(_lastDebugTile);
        }

        private void Update()
        {
            if (WorldManager.ClientWorld == null || !WorldManager.ClientWorld.IsCreated)
                return;
            
            InstantiateTextsForNewConnections();
            DrawDebugTiles();
            DrawDebugAttacks();
        }

        private void LateUpdate()
        {
            if (WorldManager.ClientWorld == null || !WorldManager.ClientWorld.IsCreated)
                return;
            
            UpdateNamesPositions();
        }
        
        
        // ReSharper disable Unity.PerformanceAnalysis
        private void InstantiateTextsForNewConnections()
        {
            var entityManager = WorldManager.ClientWorld.EntityManager;

            var entities = entityManager.CreateEntityQuery(typeof(LocalTransform), typeof(Player), typeof(DebugColor)).ToEntityArray(Allocator.Temp);

            if (entities.Length > _playerNames.Count)
            {
                foreach (var entity in entities)
                {
                    if (_playerNames.ContainsKey(entity))
                        continue;

                    var playerText = Instantiate(_playerTextPrefab, gameObject.transform);
                    playerText.SetActive(false);
                    var textMeshPro = playerText.GetComponent<TMP_Text>();

                    var debugColor = entityManager.GetComponentData<DebugColor>(entity);
                    textMeshPro.color = new Color(debugColor.Value.x, debugColor.Value.y, debugColor.Value.z);

                    var player = entityManager.GetComponentData<Player>(entity);
                    textMeshPro.text = player.Name.ConvertToString();

                    _playerNames.Add(entity, playerText);
                }
            }

            entities.Dispose();
        }

        private void UpdateNamesPositions()
        {
            var entityManager = WorldManager.ClientWorld.EntityManager;

            foreach (var player in _playerNames.ToList())
            {
                if (entityManager.Exists(player.Key))
                {
                    var lt = entityManager.GetComponentData<LocalTransform>(player.Key);
                    var pos = (Vector3)lt.Position + new Vector3(0, 0.5f, -1);
                    player.Value.transform.position = pos;
                    player.Value.SetActive(true);
                }
                else
                {
                    Destroy(player.Value);
                    _playerNames.Remove(player.Key);
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (WorldManager.ClientWorld == null || !WorldManager.ClientWorld.IsCreated)
                return;

            //DrawDebugAttacks();
            //DrawDebugTiles();
        }

        private void DrawDebugAttacks()
        {
            var entityManager = WorldManager.ClientWorld.EntityManager;

            var entities = entityManager.CreateEntityQuery(typeof(Attack), typeof(ReceiveRpcCommandRequest)).ToEntityArray(Allocator.Temp);

            foreach (var entity in entities)
            {
                var attack = entityManager.GetComponentData<Attack>(entity);

                var entitySource = Utils.GetEntityPrefab(attack.NetworkIdSource, entityManager);
                var DebugColor = entityManager.GetComponentData<DebugColor>(entitySource);

                attack.End.y = 1.6f;
                var gameObject = Instantiate(_debugTilePrefab, math.round(attack.End), Quaternion.identity);
                gameObject.GetComponentInChildren<SpriteRenderer>().color = new Color(DebugColor.Value.x, DebugColor.Value.y, DebugColor.Value.z);
                Destroy(gameObject, 1.0f);
            }

            //UI should not Consume this entities
            entityManager.DestroyEntity(entities);

            entities.Dispose();
        }

        private void DrawDebugTiles()
        {
            var entityManager = WorldManager.ClientWorld.EntityManager;

            var entities = entityManager.CreateEntityQuery(typeof(RpcTile), typeof(ReceiveRpcCommandRequest)).ToEntityArray(Allocator.Temp);

            foreach (var entity in entities)
            {
                _lastTile = entityManager.GetComponentData<RpcTile>(entity);
                _receivedTile = true;

                if (!_lastDebugTile.activeSelf)
                    _lastDebugTile.SetActive(true);

                _lastDebugTile.transform.position = math.round(_lastTile.End);
            }

            entityManager.DestroyEntity(entities);

            if (_receivedTile)
            {
                //Gizmos.color = Color.gray;
                //Gizmos.DrawCube(math.round(_lastTile.End), new float3(1, 0.1f, 1));
            }

            entities.Dispose();
        }
    }
}