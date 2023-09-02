using UnityEngine;

namespace Natrium
{
    public class FPS : MonoBehaviour
    {
        public int targetFrameRate = 30;

        private void Update()
        {
            if (Application.targetFrameRate != targetFrameRate)
                Set(targetFrameRate);
        }

        private static void Set(int target)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = target;
        }
    }
}