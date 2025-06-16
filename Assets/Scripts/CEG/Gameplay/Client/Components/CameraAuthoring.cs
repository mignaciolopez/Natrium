using Unity.Entities;
using UnityEngine;

namespace CEG.Gameplay.Client.Components
{
    public class CameraAuthoring : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        
        public class Baker : Baker<CameraAuthoring>
        {
            public override void Bake(CameraAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponentObject(entity, new MainCamera
                {
                    Camera = authoring.mainCamera
                });
                AddComponent<MainCameraTag>(entity);
            }
        }
    }

    public class MainCamera : IComponentData
    {
        public Camera Camera;
    }

    public struct MainCameraTag : IComponentData {}
}