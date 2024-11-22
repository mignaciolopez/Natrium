using System;
using Unity.NetCode;
using UnityEngine;

namespace Natrium.Shared
{
    public class FPS : MonoBehaviour
    {
        private void Start()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;
        }

        private void Update()
        {
            Application.targetFrameRate = NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;
        }
    }
}