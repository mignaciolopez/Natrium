using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace CEG.Gameplay.Shared.Mono
{
    public class WorldManager : MonoBehaviour
    {
        private enum Role
        {
            ServerAndClient = 0,
            Server,
            Client
        }
        
        [SerializeField] private string serverName = "Natrium Server";
        [SerializeField] private string clientName = "Natrium Client"; 
        private static World _serverWorld = null;
        private static World _clientWorld = null;
        private Role _role = Role.ServerAndClient;

        private void Awake()
        {
            Log.Verbose("WorldManager.Awake()");
            UpdateCurrentRole();
            InitWorlds();
        }

        private void UpdateCurrentRole()
        {
            Log.Verbose("WorldManager.UpdateCurrentRole()");
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsServer:
                case RuntimePlatform.OSXServer:
                case RuntimePlatform.LinuxServer:
                {
                    _role =  Role.Server;
                    break;
                }
#if UNITY_EDITOR
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.LinuxEditor:
                {
                    switch (MultiplayerPlayModePreferences.RequestedPlayType)
                    {
                        case ClientServerBootstrap.PlayType.Server:
                            _role = Role.Server;
                            break;
                        case ClientServerBootstrap.PlayType.ClientAndServer:
                            _role = Role.ServerAndClient;
                            break;
                        case ClientServerBootstrap.PlayType.Client:
                        default:
                            _role = Role.Client;
                            break;
                    }
                    break;
                }
#endif
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.LinuxPlayer:
                default:
                {
                    _role = Role.Client;
                    break;
                }
            }
            Log.Debug($"WorldManager.UpdateCurrentRole to: {_role}");
        }
        
        private bool InitWorlds()
        {
            Log.Verbose("WorldManager.InitWorlds()");
            foreach (var world in World.All)
            {
                if (world.Flags != WorldFlags.Game)
                    continue;
                
                Log.Debug($"WorldManager.InitWorlds Disposing: {world.Name}");
                world.Dispose();
                break;
            }

            if (_role == Role.ServerAndClient || _role == Role.Server)
            {
                Log.Debug($"WorldManager.InitWorlds Creating World: {serverName}");
                _serverWorld = ClientServerBootstrap.CreateServerWorld(serverName);

                if (_serverWorld == null)
                {
                    Log.Error($"Error ClientServerBootstrap.CreateServerWorld({serverName});", name, gameObject);
                    return false;
                }
                World.DefaultGameObjectInjectionWorld = _serverWorld;
                Log.Debug($"WorldManager.InitWorlds Set DefaultGameObjectInjectionWorld to: {_serverWorld.Name}");
            }

            if (_role == Role.ServerAndClient || _role == Role.Client)
            {
                Log.Debug($"WorldManager.InitWorlds Creating World: {clientName}");
                _clientWorld = ClientServerBootstrap.CreateClientWorld(clientName);
                if (_clientWorld == null)
                {
                    Log.Error($"Error ClientServerBootstrap.CreateClientWorld({clientName});", name, gameObject);
                    return false;
                }

                if (World.DefaultGameObjectInjectionWorld != _serverWorld)
                {
                    World.DefaultGameObjectInjectionWorld = _clientWorld;
                    Log.Debug($"WorldManager.InitWorlds Set DefaultGameObjectInjectionWorld to: {_clientWorld.Name}");
                }
            }

            return true;
        }
    }
}