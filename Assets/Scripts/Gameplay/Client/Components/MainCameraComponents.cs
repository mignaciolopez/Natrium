using Unity.Entities;
using UnityEngine;

namespace Gameplay.Client.Components
{
    public class MainCamera : IComponentData
    {
        public Camera Camera;
    }

    public struct MainCameraTag : IComponentData {}
}