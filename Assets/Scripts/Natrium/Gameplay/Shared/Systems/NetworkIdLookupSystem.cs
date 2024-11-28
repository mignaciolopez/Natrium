using System;
using Natrium.Shared;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace Natrium.Gameplay.Shared.Systems
{
    public struct NetworkIdLookup : IComponentData, IDisposable
    {
        public NativeList<Entity> PrefabsList;
        public NativeList<Entity> ConnectionList;
        
        public void Dispose()
        {
            if (PrefabsList.IsCreated)
                PrefabsList.Dispose();
            
            if (ConnectionList.IsCreated)
                ConnectionList.Dispose();
        }
        
        public Entity GetEntityPrefab(int networkId)
        {
            return networkId < PrefabsList.Length ? PrefabsList[networkId] : Entity.Null;
        }
        
        public Entity GetEntityConnection(int networkId)
        {
            return networkId < ConnectionList.Length ? ConnectionList[networkId] : Entity.Null;
        }
    }
    
    [UpdateInGroup(typeof(GhostSimulationSystemGroup), OrderFirst = true)]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial struct NetworkIdLookupSystem : ISystem, ISystemStartStop
    {
        private NetworkIdLookup _networkIdLookup;
        
        //[BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] OnCreate");
        }

        //[BurstCompile]
        public void OnStartRunning(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] OnStartRunning");

            if (!SystemAPI.HasSingleton<NetworkIdLookup>())
            {
                var entity = state.EntityManager.CreateEntity();
                Log.Verbose($"[{state.WorldUnmanaged.Name}] Creating Singleton {entity}");
                
                _networkIdLookup = new NetworkIdLookup
                {
                    PrefabsList = new NativeList<Entity>(Allocator.Persistent),
                    ConnectionList = new NativeList<Entity>(Allocator.Persistent),
                };
                
                state.EntityManager.AddComponentData(entity, _networkIdLookup);
            }
        }

        //[BurstCompile]
        public void OnStopRunning(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] OnStopRunning");
            
            if (SystemAPI.HasSingleton<NetworkIdLookup>())
            {
                _networkIdLookup.Dispose();
                var entity = SystemAPI.GetSingletonEntity<NetworkIdLookup>();
                Log.Verbose($"[{state.WorldUnmanaged.Name}] Destroying Singleton {entity}");
                state.EntityManager.DestroyEntity(entity);
            }
        }

        //[BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] OnDestroy");
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            //Updating Ghosts Entities
            _networkIdLookup.PrefabsList.Clear();

            foreach (var (ghostOwner, entity) in SystemAPI.Query<RefRO<GhostOwner>>().WithEntityAccess())
            {
                _networkIdLookup.PrefabsList.Resize(ghostOwner.ValueRO.NetworkId + 1, NativeArrayOptions.UninitializedMemory);
                _networkIdLookup.PrefabsList[ghostOwner.ValueRO.NetworkId] = entity;
            }
            
            //Updating Network Entities
            _networkIdLookup.ConnectionList.Clear();
            
            foreach (var (networkId, entity) in SystemAPI.Query<RefRO<NetworkId>>().WithEntityAccess())
            {
                _networkIdLookup.ConnectionList.Resize(networkId.ValueRO.Value + 1, NativeArrayOptions.UninitializedMemory);
                _networkIdLookup.ConnectionList[networkId.ValueRO.Value] = entity;
            }
        }
    }
}