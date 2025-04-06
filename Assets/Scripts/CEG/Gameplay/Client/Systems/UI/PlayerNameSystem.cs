using CEG.Gameplay.Client.Components.UI;
using CEG.Gameplay.Shared.Components;
using TMPro;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections;

namespace CEG.Gameplay.Client.Systems.UI
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class PlayerNameSystem : SystemBase
    {
        private GameObject _uiCanvas;
        private GameObject _playerTextPrefab;

        protected override void OnCreate()
        {
            base.OnCreate();
            Log.Verbose("OnCreate");
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            Log.Verbose("OnStartRunning");
            
            _uiCanvas = GameObject.FindGameObjectWithTag("CanvasWorldSpace");
            _playerTextPrefab = Object.FindFirstObjectByType<PlayerTextPrefabAuthoring>().prefab;
            
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
            var ecb = new EntityCommandBuffer(WorldUpdateAllocator);
            
            foreach (var (playerName, playerNameTMPTextOffset, localTransform, entity)
                     in SystemAPI.Query<RefRO<PlayerName>, RefRO<PlayerNameTMPTextProperties>, RefRO<LocalTransform>>()
                         .WithNone<PlayerNameTMPTextInitialized>()
                         .WithEntityAccess())
            {
                //ToDo: Send this from the server
                //Move the component PlayerNameTMPTextProperties to shared
                //Make it a ghost Component

                var gameObject = Object.Instantiate(_playerTextPrefab, _uiCanvas.transform);
                gameObject.transform.position = localTransform.ValueRO.Position + playerNameTMPTextOffset.ValueRO.Offset;
                
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

                playerNameTMPText.GameObject.transform.position = localTransform.ValueRO.Position + playerNameTMPTextOffset.ValueRO.Offset;
            }
        }
    }
}
