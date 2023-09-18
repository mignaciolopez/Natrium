using System.Collections.Generic;
using Natrium.Gameplay.Components;
using Natrium.Shared.Systems;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

namespace Natrium.Gameplay.UI.Mono
{
    public class UI : MonoBehaviour
    {
        private World _clientWorld;

        [SerializeField] private GameObject playerTextPrefab;

        private Dictionary<Entity, GameObject> _playerNames;
        private void OnEnable()
        {
            EventSystem.Subscribe(Shared.Events.OnUIPrimaryClick, OnUIPrimaryClick);
            foreach (var world in World.All)
            {
                if(world.Name == "ClientWorld")
                    _clientWorld = world;
            }

            _playerNames = new Dictionary<Entity, GameObject>();
        }
        
        private void OnDisable()
        {
            EventSystem.UnSubscribe(Shared.Events.OnUIPrimaryClick, OnUIPrimaryClick);
        }

        private void Update()
        {
            if (_clientWorld == null || !_clientWorld.IsCreated)
                return;

            InstantiateTextsForNewConnections();
        }

        private void LateUpdate()
        {
            if (_clientWorld == null || !_clientWorld.IsCreated)
                return;
            
            UpdateNamesPositions();
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void InstantiateTextsForNewConnections()
        {
            var entityManager = _clientWorld.EntityManager;
            var inGameQuery = entityManager.CreateEntityQuery(typeof(NetworkStreamInGame)).ToEntityArray(Allocator.Temp);

            if (inGameQuery.Length == _playerNames.Count)
            {
                inGameQuery.Dispose();
                return;
            }

            var entities = entityManager.CreateEntityQuery(typeof(LocalTransform), typeof(Player)).ToEntityArray(Allocator.Temp);
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

            entities.Dispose();
            inGameQuery.Dispose();
        }

        private void UpdateNamesPositions()
        {
            var entityManager = _clientWorld.EntityManager;

            foreach (var player in _playerNames)
            {
                var lt = entityManager.GetComponentData<LocalTransform>(player.Key);
                var pos = (Vector3)lt.Position + new Vector3(0, 0.1f, -1);
                player.Value.transform.position = pos;
                player.Value.SetActive(true);
            }
        }
        private static void OnUIPrimaryClick(Shared.Stream stream)
        {
            
        }

        private void OnDrawGizmos()
        {
            if (_clientWorld == null || !_clientWorld.IsCreated)
                return;

            DrawClickHits();
        }

        private void DrawClickHits()
        {
            var entityManager = _clientWorld.EntityManager;

            var entities = entityManager.CreateEntityQuery(typeof(Hit), typeof(LocalTransform), typeof(GhostOwner)).ToEntityArray(Allocator.Temp);

            foreach (var entity in entities)
            {
                var hit = entityManager.GetComponentData<Hit>(entity);
                var lt = entityManager.GetComponentData<LocalTransform>(entity);

                Gizmos.color = Color.gray;
                Gizmos.DrawCube(math.round(hit.End), new float3(1, 0.1f, 1));

                var end = float3.zero;

                if (hit.NetworkIdTarget != 0)
                {
                    var entitiesTarget = entityManager.CreateEntityQuery(typeof(GhostOwner), typeof(LocalTransform)).ToEntityArray(Allocator.Temp);
                    foreach (var entityTarget in entitiesTarget)
                    {
                        var goTarget = entityManager.GetComponentData<GhostOwner>(entityTarget);

                        if (goTarget.NetworkId != hit.NetworkIdTarget)
                            continue;

                        end = entityManager.GetComponentData<LocalTransform>(entityTarget).Position;
                        break;
                    }

                    entitiesTarget.Dispose();
                }
                else
                {
                    end = hit.End;
                }

                if (end is not { x: 0, y: 0, z: 0 })
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawLine(lt.Position, end);
                }
            }
            
            entities.Dispose();
        }
    }
}