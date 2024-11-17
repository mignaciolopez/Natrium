using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using Natrium.Shared;

namespace Natrium.Gameplay.Shared
{
    public enum Role
    {
        ServerAndClient = 0,
        Server,
        Client
    }

    public class WorldManager : MonoBehaviour
    {
        private Role _role = Role.ServerAndClient;
        public const string SERVER_NAME = "Natrium Server"; 
        public const string CLIENT_NAME = "Natrium Client"; 
        public static World ServerWorld = null;
        public static World ClientWorld = null;

        private void Awake()
        {
            Log.Verbose("WorldManager.Awake()");
            UpdateCurrentRole();
            InitWorlds();
        }

        private bool InitWorlds()
        {
            Log.Verbose("WorldManager.InitWorlds()");
            foreach (var world in World.All)
            {
                if (world.Flags == WorldFlags.Game)
                {
                    Log.Debug($"WorldManager.InitWorlds Disposing: {world.Name}");
                    world.Dispose();
                    break;
                }
            }

            if (_role == Role.ServerAndClient || _role == Role.Server)
            {
                Log.Debug($"WorldManager.InitWorlds Creating World: {SERVER_NAME}");
                ServerWorld = ClientServerBootstrap.CreateServerWorld(SERVER_NAME);

                if (ServerWorld == null)
                {
                    Log.Error($"Error ClientServerBootstrap.CreateServerWorld({SERVER_NAME});", name, gameObject);
                    return false;
                }
                World.DefaultGameObjectInjectionWorld = ServerWorld;
                Log.Debug($"WorldManager.InitWorlds Set DefaultGameObjectInjectionWorld to: {ServerWorld.Name}");
            }

            if (_role == Role.ServerAndClient || _role == Role.Client)
            {
                Log.Debug($"WorldManager.InitWorlds Creating World: {CLIENT_NAME}");
                ClientWorld = ClientServerBootstrap.CreateClientWorld(CLIENT_NAME);
                if (ClientWorld == null)
                {
                    Log.Error($"Error ClientServerBootstrap.CreateClientWorld({CLIENT_NAME});", name, gameObject);
                    return false;
                }

                if (World.DefaultGameObjectInjectionWorld != ServerWorld)
                {
                    World.DefaultGameObjectInjectionWorld = ClientWorld;
                    Log.Debug($"WorldManager.InitWorlds Set DefaultGameObjectInjectionWorld to: {ClientWorld.Name}");
                }
            }

            return true;
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
                        if (MultiplayerPlayModePreferences.RequestedPlayType == ClientServerBootstrap.PlayType.Server)
                            _role = Role.Server;
                        else if (MultiplayerPlayModePreferences.RequestedPlayType == ClientServerBootstrap.PlayType.ClientAndServer)
                            _role = Role.ServerAndClient;
                        else
                            _role = Role.Client;                    
                        break;
                    }
#endif
                default:
                    {
                        _role = Role.Client;
                        break;
                    }
            }
            Log.Debug($"WorldManager.UpdateCurrentRole to: {_role}");
        }
    }
}