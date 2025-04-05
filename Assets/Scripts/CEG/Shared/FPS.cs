using Unity.NetCode;
using UnityEngine;

namespace CEG.Shared
{
    public class FPS : MonoBehaviour
    {
        [SerializeField] private int targetFrameRate = 60;
        [SerializeField] private bool useNetCodeConfig;
        private int _simulationTickRate;
        private int _currentTargetFrame;
        
        private void Start()
        {
            QualitySettings.vSyncCount = 0;

            _simulationTickRate = NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;
            _currentTargetFrame = useNetCodeConfig ? _simulationTickRate : targetFrameRate;
            
            Application.targetFrameRate = _currentTargetFrame;
        }

        private void Update()
        {
            if (useNetCodeConfig)
            {
                if (_currentTargetFrame == _simulationTickRate)
                    return;
                
                _currentTargetFrame = _simulationTickRate;
            }
            else
            {
                if (_currentTargetFrame == targetFrameRate)
                    return;
                
                _currentTargetFrame = targetFrameRate;
            }

            Application.targetFrameRate = _currentTargetFrame;
        }
    }
}