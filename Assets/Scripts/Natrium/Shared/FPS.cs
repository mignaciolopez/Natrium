using UnityEngine;

namespace Natrium.Shared
{
    public class FPS : MonoBehaviour
    {
        public int targetFrameRate = 30;

        private void Update()
        {
            if (Application.targetFrameRate != targetFrameRate)
                Set(targetFrameRate);
        }

        public void Set(int target)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = target;
        }
    }
}