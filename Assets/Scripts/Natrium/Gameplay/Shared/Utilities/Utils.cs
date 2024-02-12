using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Natrium.Gameplay.Shared.Components;
using UnityEngine;

namespace Natrium.Gameplay.Shared.Utilities
{
    public static class Utils
    {
        public static Entity GetEntityPrefab(int nidValue, EntityManager entityManager)
        {
            var e = Entity.Null;
            var found = false;

            var entities = entityManager.CreateEntityQuery(typeof(GhostOwner), typeof(Player)).ToEntityArray(Allocator.Temp);

            foreach (var entity in entities)
            {
                var ghostOwner = entityManager.GetComponentData<GhostOwner>(entity);
                if (ghostOwner.NetworkId == nidValue)
                {
                    e = entity;
                    found = true;
                    break;
                }
            }

            if (!found)
                Debug.LogError($"{entityManager.World} NetworkID: {nidValue} not found! Entities in Query: {entities.Length}");

            entities.Dispose();

            return e;
        }

        public static Entity GetEntityConnection(int nidValue, EntityManager entityManager)
        {
            var e = Entity.Null;
            var found = false;

            var entities = entityManager.CreateEntityQuery(typeof(NetworkId), typeof(NetworkStreamInGame)).ToEntityArray(Allocator.Temp);

            foreach (var entity in entities)
            {
                var networkId = entityManager.GetComponentData<NetworkId>(entity);
                if (networkId.Value == nidValue)
                {
                    e = entity;
                    found = true;
                    break;
                }
            }

            if (!found)
                Debug.LogError($"{entityManager.World} NetworkID: {nidValue} not found! Entities in Query: {entities.Length}");

            entities.Dispose();

            return e;
        }
    }
}
