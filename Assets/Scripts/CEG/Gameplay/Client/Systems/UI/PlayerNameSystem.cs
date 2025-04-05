using CEG.Gameplay.Client.Components.UI;
using CEG.Gameplay.Shared.Components;
using TMPro;
using Unity.Entities;
using CEG.Shared;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

namespace CEG.Gameplay.Client.Systems.UI
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class PlayerNameSystem : SystemBase
    {
        private GameObject _uiCanvas;
        private GameObject _playerTextPrefab;

        private ComponentLookup<PlayerNameTMPTextProperties> _tmpTextPropertiesComponentLookup;
        
        protected override void OnCreate()
        {
            base.OnCreate();
            Log.Verbose("OnCreate");
            
            _tmpTextPropertiesComponentLookup = GetComponentLookup<PlayerNameTMPTextProperties>();
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            Log.Verbose("OnStartRunning");
            
            _uiCanvas = GameObject.FindGameObjectWithTag("CanvasWorldSpace");
            _playerTextPrefab = GameObject.FindFirstObjectByType<PlayerTextPrefabAuthoring>().prefab;
            
            InstantiatePlayerNames();
        }

        protected override void OnStopRunning()
        {
            base.OnStopRunning();
            Log.Verbose("OnStopRunning");

            RemovePlayerNames();

            _uiCanvas = null;
            _playerTextPrefab = null;
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            Log.Verbose("OnDestroy");
        }
        
        protected override void OnUpdate()
        {
            InstantiatePlayerNames();
            UpdatePlayerNamesPositions();
        }

        private void InstantiatePlayerNames()
        {
            _tmpTextPropertiesComponentLookup.Update(this);
            var ecb = new EntityCommandBuffer(WorldUpdateAllocator);
            
            foreach (var (playerName, localTransform, entity)
                     in SystemAPI.Query<RefRO<PlayerName>, RefRO<LocalTransform>>()
                         .WithNone<PlayerNameTMPTextInitialized>()
                         .WithEntityAccess())
            {
                //ToDo: Send this from the server
                //Move the component PlayerNameTMPTextProperties to shared
                //Make it a ghost Component

                var offset = float3.zero;
                if (_tmpTextPropertiesComponentLookup.HasComponent(entity))
                {
                    var playerNameTMPTextOffset = _tmpTextPropertiesComponentLookup[entity];
                    playerNameTMPTextOffset.Offset = new float3(0, 0, -1.0f);
                    _tmpTextPropertiesComponentLookup[entity] = playerNameTMPTextOffset;
                    offset = playerNameTMPTextOffset.Offset;
                }
                
                var gameObject = GameObject.Instantiate(_playerTextPrefab, _uiCanvas.transform);
                gameObject.transform.position = localTransform.ValueRO.Position + offset;
                
                var tmpText = gameObject.GetComponent<TMP_Text>();
                tmpText.text = playerName.ValueRO.Value.ToString();
                
                ecb.AddComponent(entity, new PlayerNameTMPText
                {
                    GameObject = gameObject,
                    Value = tmpText,
                });

                ecb.AddComponent<PlayerNameTMPTextInitialized>(entity);
            }
            
            ecb.Playback(EntityManager);
            ecb.Dispose();
        }

        private void RemovePlayerNames()
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            
            foreach (var (playerNameTMPText, entity) 
                     in SystemAPI.Query<PlayerNameTMPText>()
                         .WithAll<PlayerNameTMPTextInitialized>()
                         .WithEntityAccess())
            {
                playerNameTMPText.Dispose();
                
                ecb.RemoveComponent<PlayerNameTMPText>(entity);
                ecb.RemoveComponent<PlayerNameTMPTextInitialized>(entity);
            }
            
            ecb.Playback(EntityManager);
            ecb.Dispose();
        }

        private void UpdatePlayerNamesPositions()
        {
            _tmpTextPropertiesComponentLookup.Update(this);
            
            foreach (var (playerNameTMPText, localTransform, playerNameTMPTextOffset, entity) 
                     in SystemAPI.Query<PlayerNameTMPText, RefRO<LocalTransform>, RefRO<PlayerNameTMPTextProperties>>()
                         .WithAll<PlayerNameTMPTextInitialized>()
                         .WithEntityAccess())
            {
                if (playerNameTMPText.GameObject == null)
                {
                    Log.Warning($"{entity} has PlayerNameTMPText with a null GameObject");
                    continue;
                }
                
                var offset = float3.zero;
                if (_tmpTextPropertiesComponentLookup.HasComponent(entity))
                    offset = _tmpTextPropertiesComponentLookup[entity].Offset;
                
                playerNameTMPText.GameObject.transform.position = localTransform.ValueRO.Position + offset;
            }
        }
    }
}
