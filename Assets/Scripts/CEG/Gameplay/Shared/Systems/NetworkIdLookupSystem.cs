using System;
using CEG.Shared;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace CEG.Gameplay.Shared.Systems
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
        private Entity _singletonEntity;
        
        //[BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] OnCreate");
        }

        //[BurstCompile]
        public void OnStartRunning(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] OnStartRunning");

            _singletonEntity = state.EntityManager.CreateEntity();

            state.EntityManager.AddComponentData(_singletonEntity, new NetworkIdLookup
            {
                PrefabsList = new NativeList<Entity>(Allocator.Persistent),
                ConnectionList = new NativeList<Entity>(Allocator.Persistent),
            });
        }

        //[BurstCompile]
        public void OnStopRunning(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] OnStopRunning");

            if (SystemAPI.TryGetSingletonRW<NetworkIdLookup>(out var networkIdLookup))
            {
                networkIdLookup.ValueRW.Dispose();
                state.EntityManager.DestroyEntity(_singletonEntity);
            }
        }

        //[BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            Log.Verbose($"[{state.WorldUnmanaged.Name}] OnDestroy");
            OnStopRunning(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingletonRW<NetworkIdLookup>(out var networkIdLookup))
            {
                return;
            }
                
            //Updating Ghosts Entities
            networkIdLookup.ValueRW.PrefabsList.Clear();

            foreach (var (ghostOwner, entity) in SystemAPI.Query<RefRO<GhostOwner>>().WithEntityAccess())
            {
                if (networkIdLookup.ValueRW.PrefabsList.Length < ghostOwner.ValueRO.NetworkId + 1)
                    networkIdLookup.ValueRW.PrefabsList.Resize(ghostOwner.ValueRO.NetworkId + 1, NativeArrayOptions.UninitializedMemory);
                networkIdLookup.ValueRW.PrefabsList[ghostOwner.ValueRO.NetworkId] = entity;
            }
            
            //Updating Network Entities
            networkIdLookup.ValueRW.ConnectionList.Clear();
            
            foreach (var (networkId, entity) in SystemAPI.Query<RefRO<NetworkId>>().WithEntityAccess())
            {
                if (networkIdLookup.ValueRW.ConnectionList.Length < networkId.ValueRO.Value + 1)
                    networkIdLookup.ValueRW.ConnectionList.Resize(networkId.ValueRO.Value + 1, NativeArrayOptions.UninitializedMemory);
                networkIdLookup.ValueRW.ConnectionList[networkId.ValueRO.Value] = entity;
            }
        }
    }
}