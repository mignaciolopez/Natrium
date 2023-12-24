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

namespace Natrium.Gameplay.Client.UI
{
    public class UI : MonoBehaviour
    {
        [SerializeField] private GameObject playerTextPrefab;

        private Dictionary<Entity, GameObject> _playerNames;
        private void OnEnable()
        {
            _playerNames = new Dictionary<Entity, GameObject>();
        }
        
        private void OnDisable()
        {
            
        }

        private void Update()
        {
            if (WorldManager.ClientWorld == null || !WorldManager.ClientWorld.IsCreated)
                return;
            
            InstantiateTextsForNewConnections();
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

            var entities = entityManager.CreateEntityQuery(typeof(LocalTransform), typeof(Player)).ToEntityArray(Allocator.Temp);

            if (entities.Length > _playerNames.Count)
            {
                foreach (var entity in entities)
                {
                    if (_playerNames.ContainsKey(entity))
                        continue;

                    var playerText = Instantiate(playerTextPrefab, gameObject.transform);
                    playerText.SetActive(false);
                    var textMeshPro = playerText.GetComponent<TMP_Text>();

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

            DebugDrawAttacks();
            DebugDrawTiles();
        }

        private void DebugDrawAttacks()
        {
            var entityManager = WorldManager.ClientWorld.EntityManager;

            var entities = entityManager.CreateEntityQuery(typeof(Attack), typeof(LocalTransform), typeof(GhostOwner)).ToEntityArray(Allocator.Temp);

            foreach (var entity in entities)
            {
                var attack = entityManager.GetComponentData<Attack>(entity);
                var lt = entityManager.GetComponentData<LocalTransform>(entity);

                Gizmos.color = Color.gray;
                Gizmos.DrawCube(math.round(attack.End), new float3(1, 0.1f, 1));

                var end = float3.zero;

                if (attack.NetworkIdTarget != 0)
                {
                    var entitiesTarget = entityManager.CreateEntityQuery(typeof(GhostOwner), typeof(LocalTransform)).ToEntityArray(Allocator.Temp);
                    foreach (var entityTarget in entitiesTarget)
                    {
                        var goTarget = entityManager.GetComponentData<GhostOwner>(entityTarget);

                        if (goTarget.NetworkId != attack.NetworkIdTarget)
                            continue;

                        end = entityManager.GetComponentData<LocalTransform>(entityTarget).Position;
                        break;
                    }

                    entitiesTarget.Dispose();
                }
                else
                {
                    end = attack.End;
                }

                if (end is not { x: 0, y: 0, z: 0 })
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawLine(lt.Position, end);
                }
            }
            
            entities.Dispose();
        }

        private void DebugDrawTiles()
        {
            var entityManager = WorldManager.ClientWorld.EntityManager;

            var entities = entityManager.CreateEntityQuery(typeof(Tile), typeof(LocalTransform), typeof(GhostOwner)).ToEntityArray(Allocator.Temp);

            foreach (var entity in entities)
            {
                var tile = entityManager.GetComponentData<Attack>(entity);
                var lt = entityManager.GetComponentData<LocalTransform>(entity);

                Gizmos.color = Color.gray;
                Gizmos.DrawCube(math.round(tile.End), new float3(1, 0.1f, 1));

                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(lt.Position, tile.End);
            }

            entities.Dispose();
        }
    }
}