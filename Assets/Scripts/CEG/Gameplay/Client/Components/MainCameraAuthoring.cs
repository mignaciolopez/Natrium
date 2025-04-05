using Unity.Entities;
using UnityEngine;

namespace CEG.Gameplay.Client.Components
{
    public class MainCameraAuthoring : MonoBehaviour
    {
        public class Baker : Baker<MainCameraAuthoring>
        {
            public override void Bake(MainCameraAuthoring authoring)
            {
                var e = GetEntity(TransformUsageFlags.Dynamic);
                AddComponentObject(e, new MainCamera());
                AddComponent<MainCameraTag>(e);
            }
        }
    }

    public class MainCamera : IComponentData
    {
        public Camera Camera;
    }

    public struct MainCameraTag : IComponentData {}
}