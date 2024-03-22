//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.7.0
//     from Assets/Settings/Input/InputActions.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Natrium.Settings.Input
{
    public partial class @InputActions: IInputActionCollection2, IDisposable
    {
        public InputActionAsset asset { get; }
        public @InputActions()
        {
            asset = InputActionAsset.FromJson(@"{
    ""name"": ""InputActions"",
    ""maps"": [
        {
            ""name"": ""Map_Gameplay"",
            ""id"": ""b17f6048-0eca-48d4-80d5-e61756f314da"",
            ""actions"": [
                {
                    ""name"": ""Axn_PlayerMove"",
                    ""type"": ""Value"",
                    ""id"": ""ead4689b-9687-4efe-bdf4-69a9d8774a0e"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Axn_MouseRealease"",
                    ""type"": ""Button"",
                    ""id"": ""0992c1a8-b81a-4613-94c5-64d8ad09b1d9"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Axn_MousePosition"",
                    ""type"": ""Value"",
                    ""id"": ""25bbc420-1c2b-43d9-bd49-31887ee5e81d"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=1)"",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Axn_Ping"",
                    ""type"": ""Button"",
                    ""id"": ""1bb8d660-6a67-4b1a-bb63-55d62869092b"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""Arrows"",
                    ""id"": ""9eaa3c65-ade9-4adc-80b7-6611fd866154"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Axn_PlayerMove"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""4bb6da9d-3dfe-48e7-a59c-18f2074110eb"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Axn_PlayerMove"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""917dc226-caf6-4c35-a93c-8ff388fa93c0"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Axn_PlayerMove"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""18fa5ad3-ef03-4a66-a149-1476fda8018d"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Axn_PlayerMove"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""1198c8ee-0da3-480e-934e-9bd806201a58"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Axn_PlayerMove"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""ea14f93b-14b8-4dbd-9273-3a492951676a"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": ""Press(behavior=1)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Axn_MouseRealease"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""6f45c5cc-5b37-451c-915b-1a3dfa2cadc8"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Axn_MousePosition"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""0ffebaef-fec3-418c-822a-76e9e1bbf0a6"",
                    ""path"": ""<Keyboard>/f2"",
                    ""interactions"": ""Press(behavior=1)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Axn_Ping"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""Map_Login"",
            ""id"": ""f1016748-525e-4398-93dd-e678078e2d06"",
            ""actions"": [
                {
                    ""name"": ""Axn_Enter"",
                    ""type"": ""Button"",
                    ""id"": ""97754822-0dc1-40d0-a5ad-e5013e2e3528"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Axn_Escape"",
                    ""type"": ""Button"",
                    ""id"": ""8716fad9-d1d3-4b1b-9306-4f9e8b430605"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""08911c33-b93d-40f1-93c1-a71ac6ad14ac"",
                    ""path"": ""<Keyboard>/enter"",
                    ""interactions"": ""Press(behavior=1)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Axn_Enter"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""5bd01805-1a8e-4789-ab10-2273b4a613e8"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": ""Press(behavior=1)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Axn_Escape"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""Map_UI"",
            ""id"": ""751ff963-f42b-4651-a9a6-3644752f6686"",
            ""actions"": [
                {
                    ""name"": ""Axn_FullScreenToggle"",
                    ""type"": ""Button"",
                    ""id"": ""c8136f3c-62ae-4ce6-b2ca-fc74aaea2f9a"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""746d9e8d-8544-4e26-a8ed-1871b0faed29"",
                    ""path"": ""<Keyboard>/f11"",
                    ""interactions"": ""Press(behavior=1)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Axn_FullScreenToggle"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
            // Map_Gameplay
            m_Map_Gameplay = asset.FindActionMap("Map_Gameplay", throwIfNotFound: true);
            m_Map_Gameplay_Axn_PlayerMove = m_Map_Gameplay.FindAction("Axn_PlayerMove", throwIfNotFound: true);
            m_Map_Gameplay_Axn_MouseRealease = m_Map_Gameplay.FindAction("Axn_MouseRealease", throwIfNotFound: true);
            m_Map_Gameplay_Axn_MousePosition = m_Map_Gameplay.FindAction("Axn_MousePosition", throwIfNotFound: true);
            m_Map_Gameplay_Axn_Ping = m_Map_Gameplay.FindAction("Axn_Ping", throwIfNotFound: true);
            // Map_Login
            m_Map_Login = asset.FindActionMap("Map_Login", throwIfNotFound: true);
            m_Map_Login_Axn_Enter = m_Map_Login.FindAction("Axn_Enter", throwIfNotFound: true);
            m_Map_Login_Axn_Escape = m_Map_Login.FindAction("Axn_Escape", throwIfNotFound: true);
            // Map_UI
            m_Map_UI = asset.FindActionMap("Map_UI", throwIfNotFound: true);
            m_Map_UI_Axn_FullScreenToggle = m_Map_UI.FindAction("Axn_FullScreenToggle", throwIfNotFound: true);
        }

        public void Dispose()
        {
            UnityEngine.Object.Destroy(asset);
        }

        public InputBinding? bindingMask
        {
            get => asset.bindingMask;
            set => asset.bindingMask = value;
        }

        public ReadOnlyArray<InputDevice>? devices
        {
            get => asset.devices;
            set => asset.devices = value;
        }

        public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

        public bool Contains(InputAction action)
        {
            return asset.Contains(action);
        }

        public IEnumerator<InputAction> GetEnumerator()
        {
            return asset.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Enable()
        {
            asset.Enable();
        }

        public void Disable()
        {
            asset.Disable();
        }

        public IEnumerable<InputBinding> bindings => asset.bindings;

        public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
        {
            return asset.FindAction(actionNameOrId, throwIfNotFound);
        }

        public int FindBinding(InputBinding bindingMask, out InputAction action)
        {
            return asset.FindBinding(bindingMask, out action);
        }

        // Map_Gameplay
        private readonly InputActionMap m_Map_Gameplay;
        private List<IMap_GameplayActions> m_Map_GameplayActionsCallbackInterfaces = new List<IMap_GameplayActions>();
        private readonly InputAction m_Map_Gameplay_Axn_PlayerMove;
        private readonly InputAction m_Map_Gameplay_Axn_MouseRealease;
        private readonly InputAction m_Map_Gameplay_Axn_MousePosition;
        private readonly InputAction m_Map_Gameplay_Axn_Ping;
        public struct Map_GameplayActions
        {
            private @InputActions m_Wrapper;
            public Map_GameplayActions(@InputActions wrapper) { m_Wrapper = wrapper; }
            public InputAction @Axn_PlayerMove => m_Wrapper.m_Map_Gameplay_Axn_PlayerMove;
            public InputAction @Axn_MouseRealease => m_Wrapper.m_Map_Gameplay_Axn_MouseRealease;
            public InputAction @Axn_MousePosition => m_Wrapper.m_Map_Gameplay_Axn_MousePosition;
            public InputAction @Axn_Ping => m_Wrapper.m_Map_Gameplay_Axn_Ping;
            public InputActionMap Get() { return m_Wrapper.m_Map_Gameplay; }
            public void Enable() { Get().Enable(); }
            public void Disable() { Get().Disable(); }
            public bool enabled => Get().enabled;
            public static implicit operator InputActionMap(Map_GameplayActions set) { return set.Get(); }
            public void AddCallbacks(IMap_GameplayActions instance)
            {
                if (instance == null || m_Wrapper.m_Map_GameplayActionsCallbackInterfaces.Contains(instance)) return;
                m_Wrapper.m_Map_GameplayActionsCallbackInterfaces.Add(instance);
                @Axn_PlayerMove.started += instance.OnAxn_PlayerMove;
                @Axn_PlayerMove.performed += instance.OnAxn_PlayerMove;
                @Axn_PlayerMove.canceled += instance.OnAxn_PlayerMove;
                @Axn_MouseRealease.started += instance.OnAxn_MouseRealease;
                @Axn_MouseRealease.performed += instance.OnAxn_MouseRealease;
                @Axn_MouseRealease.canceled += instance.OnAxn_MouseRealease;
                @Axn_MousePosition.started += instance.OnAxn_MousePosition;
                @Axn_MousePosition.performed += instance.OnAxn_MousePosition;
                @Axn_MousePosition.canceled += instance.OnAxn_MousePosition;
                @Axn_Ping.started += instance.OnAxn_Ping;
                @Axn_Ping.performed += instance.OnAxn_Ping;
                @Axn_Ping.canceled += instance.OnAxn_Ping;
            }

            private void UnregisterCallbacks(IMap_GameplayActions instance)
            {
                @Axn_PlayerMove.started -= instance.OnAxn_PlayerMove;
                @Axn_PlayerMove.performed -= instance.OnAxn_PlayerMove;
                @Axn_PlayerMove.canceled -= instance.OnAxn_PlayerMove;
                @Axn_MouseRealease.started -= instance.OnAxn_MouseRealease;
                @Axn_MouseRealease.performed -= instance.OnAxn_MouseRealease;
                @Axn_MouseRealease.canceled -= instance.OnAxn_MouseRealease;
                @Axn_MousePosition.started -= instance.OnAxn_MousePosition;
                @Axn_MousePosition.performed -= instance.OnAxn_MousePosition;
                @Axn_MousePosition.canceled -= instance.OnAxn_MousePosition;
                @Axn_Ping.started -= instance.OnAxn_Ping;
                @Axn_Ping.performed -= instance.OnAxn_Ping;
                @Axn_Ping.canceled -= instance.OnAxn_Ping;
            }

            public void RemoveCallbacks(IMap_GameplayActions instance)
            {
                if (m_Wrapper.m_Map_GameplayActionsCallbackInterfaces.Remove(instance))
                    UnregisterCallbacks(instance);
            }

            public void SetCallbacks(IMap_GameplayActions instance)
            {
                foreach (var item in m_Wrapper.m_Map_GameplayActionsCallbackInterfaces)
                    UnregisterCallbacks(item);
                m_Wrapper.m_Map_GameplayActionsCallbackInterfaces.Clear();
                AddCallbacks(instance);
            }
        }
        public Map_GameplayActions @Map_Gameplay => new Map_GameplayActions(this);

        // Map_Login
        private readonly InputActionMap m_Map_Login;
        private List<IMap_LoginActions> m_Map_LoginActionsCallbackInterfaces = new List<IMap_LoginActions>();
        private readonly InputAction m_Map_Login_Axn_Enter;
        private readonly InputAction m_Map_Login_Axn_Escape;
        public struct Map_LoginActions
        {
            private @InputActions m_Wrapper;
            public Map_LoginActions(@InputActions wrapper) { m_Wrapper = wrapper; }
            public InputAction @Axn_Enter => m_Wrapper.m_Map_Login_Axn_Enter;
            public InputAction @Axn_Escape => m_Wrapper.m_Map_Login_Axn_Escape;
            public InputActionMap Get() { return m_Wrapper.m_Map_Login; }
            public void Enable() { Get().Enable(); }
            public void Disable() { Get().Disable(); }
            public bool enabled => Get().enabled;
            public static implicit operator InputActionMap(Map_LoginActions set) { return set.Get(); }
            public void AddCallbacks(IMap_LoginActions instance)
            {
                if (instance == null || m_Wrapper.m_Map_LoginActionsCallbackInterfaces.Contains(instance)) return;
                m_Wrapper.m_Map_LoginActionsCallbackInterfaces.Add(instance);
                @Axn_Enter.started += instance.OnAxn_Enter;
                @Axn_Enter.performed += instance.OnAxn_Enter;
                @Axn_Enter.canceled += instance.OnAxn_Enter;
                @Axn_Escape.started += instance.OnAxn_Escape;
                @Axn_Escape.performed += instance.OnAxn_Escape;
                @Axn_Escape.canceled += instance.OnAxn_Escape;
            }

            private void UnregisterCallbacks(IMap_LoginActions instance)
            {
                @Axn_Enter.started -= instance.OnAxn_Enter;
                @Axn_Enter.performed -= instance.OnAxn_Enter;
                @Axn_Enter.canceled -= instance.OnAxn_Enter;
                @Axn_Escape.started -= instance.OnAxn_Escape;
                @Axn_Escape.performed -= instance.OnAxn_Escape;
                @Axn_Escape.canceled -= instance.OnAxn_Escape;
            }

            public void RemoveCallbacks(IMap_LoginActions instance)
            {
                if (m_Wrapper.m_Map_LoginActionsCallbackInterfaces.Remove(instance))
                    UnregisterCallbacks(instance);
            }

            public void SetCallbacks(IMap_LoginActions instance)
            {
                foreach (var item in m_Wrapper.m_Map_LoginActionsCallbackInterfaces)
                    UnregisterCallbacks(item);
                m_Wrapper.m_Map_LoginActionsCallbackInterfaces.Clear();
                AddCallbacks(instance);
            }
        }
        public Map_LoginActions @Map_Login => new Map_LoginActions(this);

        // Map_UI
        private readonly InputActionMap m_Map_UI;
        private List<IMap_UIActions> m_Map_UIActionsCallbackInterfaces = new List<IMap_UIActions>();
        private readonly InputAction m_Map_UI_Axn_FullScreenToggle;
        public struct Map_UIActions
        {
            private @InputActions m_Wrapper;
            public Map_UIActions(@InputActions wrapper) { m_Wrapper = wrapper; }
            public InputAction @Axn_FullScreenToggle => m_Wrapper.m_Map_UI_Axn_FullScreenToggle;
            public InputActionMap Get() { return m_Wrapper.m_Map_UI; }
            public void Enable() { Get().Enable(); }
            public void Disable() { Get().Disable(); }
            public bool enabled => Get().enabled;
            public static implicit operator InputActionMap(Map_UIActions set) { return set.Get(); }
            public void AddCallbacks(IMap_UIActions instance)
            {
                if (instance == null || m_Wrapper.m_Map_UIActionsCallbackInterfaces.Contains(instance)) return;
                m_Wrapper.m_Map_UIActionsCallbackInterfaces.Add(instance);
                @Axn_FullScreenToggle.started += instance.OnAxn_FullScreenToggle;
                @Axn_FullScreenToggle.performed += instance.OnAxn_FullScreenToggle;
                @Axn_FullScreenToggle.canceled += instance.OnAxn_FullScreenToggle;
            }

            private void UnregisterCallbacks(IMap_UIActions instance)
            {
                @Axn_FullScreenToggle.started -= instance.OnAxn_FullScreenToggle;
                @Axn_FullScreenToggle.performed -= instance.OnAxn_FullScreenToggle;
                @Axn_FullScreenToggle.canceled -= instance.OnAxn_FullScreenToggle;
            }

            public void RemoveCallbacks(IMap_UIActions instance)
            {
                if (m_Wrapper.m_Map_UIActionsCallbackInterfaces.Remove(instance))
                    UnregisterCallbacks(instance);
            }

            public void SetCallbacks(IMap_UIActions instance)
            {
                foreach (var item in m_Wrapper.m_Map_UIActionsCallbackInterfaces)
                    UnregisterCallbacks(item);
                m_Wrapper.m_Map_UIActionsCallbackInterfaces.Clear();
                AddCallbacks(instance);
            }
        }
        public Map_UIActions @Map_UI => new Map_UIActions(this);
        public interface IMap_GameplayActions
        {
            void OnAxn_PlayerMove(InputAction.CallbackContext context);
            void OnAxn_MouseRealease(InputAction.CallbackContext context);
            void OnAxn_MousePosition(InputAction.CallbackContext context);
            void OnAxn_Ping(InputAction.CallbackContext context);
        }
        public interface IMap_LoginActions
        {
            void OnAxn_Enter(InputAction.CallbackContext context);
            void OnAxn_Escape(InputAction.CallbackContext context);
        }
        public interface IMap_UIActions
        {
            void OnAxn_FullScreenToggle(InputAction.CallbackContext context);
        }
    }
}
