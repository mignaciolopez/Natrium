using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TMG.NFE_Tutorial
{
    public class ClientConnectionManager : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _addressField;
        [SerializeField] private TMP_InputField _portField;
        [SerializeField] private TMP_Dropdown _connectionModeDropdown;
        [SerializeField] private TMP_Dropdown _teamDropdown;
        [SerializeField] private Button _connectButton;
        
        private ushort Port => ushort.Parse(_portField.text);
        private string Address => _addressField.text;

        private void OnEnable()
        {
            _connectionModeDropdown.onValueChanged.AddListener(OnConnectionModeChanged);
            _connectButton.onClick.AddListener(OnButtonConnect);
            OnConnectionModeChanged(_connectionModeDropdown.value);
        }

        private void OnDisable()
        {
            _connectionModeDropdown.onValueChanged.RemoveAllListeners();
            _connectButton.onClick.RemoveAllListeners();
        }

        private void OnConnectionModeChanged(int connectionMode)
        {
            string buttonLabel;
            _connectButton.enabled = true;
            
            switch (connectionMode)
            {
                case 0:
                    buttonLabel = "Start Host";
                    break;
                case 1 : 
                    buttonLabel = "Start Server";
                    break;
                case 2:
                    buttonLabel = "Start Client";
                    break;
                default:
                    buttonLabel = "<ERROR>";
                    _connectButton.enabled = false;
                    break;
            }

            var buttonText = _connectButton.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = buttonLabel;
        }

        private void OnButtonConnect()
        {
            DestroyLocalSimulationWorld();
            SceneManager.LoadScene(1);
            
            switch (_connectionModeDropdown.value)
            {
                case 0:
                    StartServer();
                    StartClient();
                    break;
                case 1:
                    StartServer();
                    break;
                case 2:
                    StartClient();
                    break;
                default:
                    Debug.LogError("Error: Unknown connection mode", gameObject);
                    break;
            }
        }

        private static void DestroyLocalSimulationWorld()
        {
            
        }

        private void StartServer()
        {
            
        }

        private void StartClient()
        {
            
        }
    }
}