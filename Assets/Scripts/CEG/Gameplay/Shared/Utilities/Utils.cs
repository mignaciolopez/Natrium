using System;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using CEG.Shared;

namespace CEG.Gameplay.Shared.Utilities
{
    public static class Utils
    {
        [Obsolete("Use Instead: SystemAPI.GetSingleton<NetworkIdLookup>().GetEntityPrefab(int NetworkId)")]
        public static Entity GetEntityPrefab(int nidValue, EntityManager entityManager)
        {
            //Log.Verbose($"[{entityManager.World}] GetEntityPrefab");
            var e = Entity.Null;
            var found = false;

            var entities = entityManager.CreateEntityQuery(typeof(GhostOwner)).ToEntityArray(Allocator.Temp);

            foreach (var entity in entities)
            {
                var ghostOwner = entityManager.GetComponentData<GhostOwner>(entity);
                if (ghostOwner.NetworkId == nidValue)
                {
                    e = entity;
                    found = true;
                    //Log.Debug($"[{entityManager.World}] Found NetworkID: {nidValue} Associated with Entity: {entity}");
                    //Log.Debug($"[{entityManager.World}] Entities in Query: {entities.Length}");
                    break;
                }
            }

            if (!found)
                Log.Error($"[{entityManager.World}] NetworkID: {nidValue} not found! Entities in Query: {entities.Length}");

            entities.Dispose();

            return e;
        }

        [Obsolete("Use Instead: SystemAPI.GetSingleton<NetworkIdLookup>().GetEntityConnection(int NetworkId)")]
        public static Entity GetEntityConnection(int nidValue, EntityManager entityManager)
        {
            //Log.Verbose($"[{entityManager.World}] GetEntityConnection");
            var e = Entity.Null;
            var found = false;

            var entities = entityManager.CreateEntityQuery(typeof(NetworkId)).ToEntityArray(Allocator.Temp);

            foreach (var entity in entities)
            {
                var networkId = entityManager.GetComponentData<NetworkId>(entity);
                if (networkId.Value == nidValue)
                {
                    e = entity;
                    found = true;
                    //Log.Debug($"[{entityManager.World}] Found NetworkID: {nidValue} Associated with Entity: {entity}");
                    //Log.Debug($"[{entityManager.World}] Entities in Query: {entities.Length}");
                    break;
                }
            }

            if (!found)
                Log.Error($"[{entityManager.World}] NetworkID: {nidValue} not found! Entities in Query: {entities.Length}");

            entities.Dispose();

            return e;
        }
    }
}
