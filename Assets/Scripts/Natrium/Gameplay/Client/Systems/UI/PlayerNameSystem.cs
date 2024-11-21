using Natrium.Gameplay.Client.Components.UI;
using Natrium.Gameplay.Shared.Components;
using TMPro;
using Unity.Entities;
using Natrium.Shared;
using Unity.Transforms;
using UnityEngine;
using System.Collections.Generic;
using Natrium.Shared.Extensions;

namespace Natrium.Gameplay.Client.Systems.UI
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class PlayerNameSystem : SystemBase
    {
        private GameObject _playerTextPrefab;
        private GameObject _UICanvas;
        private Dictionary<Entity, GameObject> _textEntities;
        private List<Entity> _namesToRemove;

        private EntityCommandBuffer _ecb;
        protected override void OnCreate()
        {
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnCreate()");
            base.OnCreate();

            _textEntities = new Dictionary<Entity, GameObject>();
            _namesToRemove = new List<Entity>();
        }

        protected override void OnStartRunning()
        {
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnStartRunning()");
            base.OnStartRunning();

            _playerTextPrefab = GameObject.FindAnyObjectByType<PlayerTextPrefabAuthoring>().Prefab;
            _UICanvas = GameObject.FindGameObjectWithTag("CanvasWorldSpace");
        }

        protected override void OnStopRunning()
        {
            Log.Verbose($"[{World.Name}] | {this.ToString()}.OnStopRunning()");
            base.OnStopRunning();

            var ecbs = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            _ecb = ecbs.CreateCommandBuffer(World.Unmanaged);

            foreach (var text in _textEntities.Values)
            {
                GameObject.Destroy(text);
            }

            _textEntities.Clear();

            foreach (var (ptt, e) in SystemAPI.Query<PlayerTextDrawnTag>().WithEntityAccess())
            {
                _ecb.RemoveComponent<PlayerTextDrawnTag>(e);
            }
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

            InstantiatePlayerNames();
            RemovePlayerNames();
            UpdatePlayerNamesPositions();
        }

        private void InstantiatePlayerNames()
        {
            foreach(var (lt, pn, dc, e) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<PlayerName>, RefRO<DebugColor>>().WithNone<PlayerTextDrawnTag>().WithEntityAccess())
            {
                var text = GameObject.Instantiate(_playerTextPrefab, _UICanvas.transform);
                text.transform.position = (Vector3)lt.ValueRO.Position + new Vector3(0, 0.5f, -1);

                var tmPro = text.GetComponent<TMP_Text>();
                tmPro.text = pn.ValueRO.Value.ToString();

                tmPro.color = dc.ValueRO.StartValue.ToColor();
                _textEntities.Add(e, text);

                _ecb.AddComponent<PlayerTextDrawnTag>(e);
            }
        }

        private void RemovePlayerNames()
        {
            foreach (var entityText in _textEntities)
            {
                if (!EntityManager.Exists(entityText.Key))
                {
                    GameObject.Destroy(entityText.Value);
                    _namesToRemove.Add(entityText.Key);
                }
            }

            foreach (var e in _namesToRemove)
            {
                _textEntities.Remove(e);
            }

            _namesToRemove.Clear();
        }

        private void UpdatePlayerNamesPositions()
        {
            foreach (var (lt, e) in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<PlayerTextDrawnTag>().WithEntityAccess())
            {
                if (_textEntities.ContainsKey(e))
                    _textEntities[e].transform.position = (Vector3)lt.ValueRO.Position + new Vector3(0, 0.5f, -1);
                else
                    Log.Error($"TMP_Text do not exist for entity: {e}");
            }
        }
    }
}
