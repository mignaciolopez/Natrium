using Natrium.Gameplay.Components;
using Natrium.Shared.Systems;
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
        private EntityManager _entityManager;
        private World _world;
        private void Start()
        {
            EventSystem.Subscribe(Shared.Events.OnUIPrimaryClick, OnUIPrimaryClick);
            foreach (var world in World.All)
            {
                if (world.Name == "ClientWorld")
                {
                    _world = world;
                    _entityManager = _world.EntityManager;
                }
            }
            //yield return new WaitForSeconds(1.0f);
        }

        private void OnDisable()
        {
            EventSystem.UnSubscribe(Shared.Events.OnUIPrimaryClick, OnUIPrimaryClick);
        }

        private static void OnUIPrimaryClick(Shared.Stream stream)
        {
            
        }

        private void OnDrawGizmos()
        {
            if (_world == null)
                return;

            DrawClickHits();
        }

        private void DrawClickHits()
        {
            var query = _entityManager.CreateEntityQuery(typeof(Hit), typeof(LocalTransform), typeof(GhostOwner));
            var entities = query.ToEntityArray(Allocator.Temp);

            foreach (var entity in entities)
            {
                var hit = _entityManager.GetComponentData<Hit>(entity);
                var lt = _entityManager.GetComponentData<LocalTransform>(entity);

                Gizmos.color = Color.gray;
                Gizmos.DrawCube(math.round(hit.End), new float3(1, 0.1f, 1));

                var end = float3.zero;
                
                if (hit.NetworkIdTarget != 0)
                {
                    var queryTarget = _entityManager.CreateEntityQuery(typeof(GhostOwner), typeof(LocalTransform), typeof(GhostOwner));
                    var entitiesTarget = queryTarget.ToEntityArray(Allocator.Temp);

                    //foreach (var (goTarget, ltTarget) in SystemAPI.Query<GhostOwner, LocalTransform>())
                    foreach (var entityTarget in entitiesTarget)
                    {
                        var goTarget = _entityManager.GetComponentData<GhostOwner>(entityTarget);
                        var ltTarget = _entityManager.GetComponentData<LocalTransform>(entityTarget);
                        
                        if (goTarget.NetworkId != hit.NetworkIdTarget) continue;

                        end = ltTarget.Position;
                        break;
                    }
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
            
            //foreach (var (hit, lt) in SystemAPI.Query<Hit, LocalTransform>().WithAll<GhostOwner>())
        }
    }
}