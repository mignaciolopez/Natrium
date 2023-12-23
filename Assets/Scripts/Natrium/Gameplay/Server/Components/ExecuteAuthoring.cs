using Unity.Entities;
using UnityEngine;

namespace Natrium.Server.Gameplay.Components
{
    public class ExecuteAuthoring : MonoBehaviour
    {
        public bool movementSystem = true;
    }

    public class ExecuteBaker : Baker<ExecuteAuthoring>
    {
        public override void Bake(ExecuteAuthoring authoring)
        {
            var e = GetEntity(TransformUsageFlags.None);

            if (authoring.movementSystem)
                AddComponent<MovementSystemExecute>(e);
        }
    }

    public struct MovementSystemExecute : IComponentData
    {
        
    }
}
