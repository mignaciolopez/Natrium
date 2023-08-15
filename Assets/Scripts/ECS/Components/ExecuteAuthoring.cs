using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace Natrium
{
    public class ExecuteAuthoring : MonoBehaviour
    {
        public bool MovementSystem = true;
    }

    public class ExecuteBaker : Baker<ExecuteAuthoring>
    {
        public override void Bake(ExecuteAuthoring authoring)
        {
            Entity e = GetEntity(TransformUsageFlags.None);

            if (authoring.MovementSystem)
                AddComponent<MovementSystemExecuteData>(e);
        }
    }

    public struct MovementSystemExecuteData : IComponentData
    {
        
    }
}
