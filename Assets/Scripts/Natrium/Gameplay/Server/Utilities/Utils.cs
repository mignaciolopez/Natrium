using Natrium.Gameplay.Shared;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Natrium.Gameplay.Shared.Components;
using UnityEngine;

namespace Natrium.Gameplay.Server.Utilities
{
    public static class Utils
    {
        private static EntityManager _entityManager = WorldManager.ServerWorld.EntityManager;
        public static Entity GetEntityPrefab(int nidValue)
        {
            _entityManager = WorldManager.ServerWorld.EntityManager;

            var e = Entity.Null;
            var found = false;

            var entities = _entityManager.CreateEntityQuery(typeof(GhostOwner), typeof(Player)).ToEntityArray(Allocator.Temp);

            foreach (var entity in entities)
            {
                var ghostOwner = _entityManager.GetComponentData<GhostOwner>(entity);
                if (ghostOwner.NetworkId == nidValue)
                {
                    e = entity;
                    found = true;
                    break;
                }
            }

            if (!found)
                Debug.LogError($"{WorldManager.ServerWorld} NetworkID: {nidValue} not found! entities Length: {entities.Length}");

            entities.Dispose();

            return e;
        }

        public static Entity GetEntityConnection(int nidValue)
        {
            _entityManager = WorldManager.ServerWorld.EntityManager;

            var e = Entity.Null;
            var found = false;

            var entities = _entityManager.CreateEntityQuery(typeof(NetworkId), typeof(NetworkStreamInGame)).ToEntityArray(Allocator.Temp);

            foreach (var entity in entities)
            {
                var networkId = _entityManager.GetComponentData<NetworkId>(entity);
                if (networkId.Value == nidValue)
                {
                    e = entity;
                    found = true;
                    break;
                }
            }

            if (!found)
                Debug.LogError($"{WorldManager.ServerWorld} NetworkID: {nidValue} not found! entities Length: {entities.Length}");

            entities.Dispose();

            return e;
        }
    }
}
