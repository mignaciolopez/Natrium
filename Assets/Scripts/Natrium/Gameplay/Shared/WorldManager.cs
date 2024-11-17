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
        public static World ServerWorld = null;
        public static World ClientWorld = null;

        private void Awake()
        {
            UpdateCurrentRole();
            InitWorlds();
        }

        private bool InitWorlds()
        {
            foreach (var world in World.All)
            {
                if (world.Flags == WorldFlags.Game)
                {
                    world.Dispose();
                    break;
                }
            }

            if (_role == Role.ServerAndClient || _role == Role.Server)
            {
                ServerWorld = ClientServerBootstrap.CreateServerWorld("Natrium Server");
                DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(ServerWorld, DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.ServerSimulation));

                if (ServerWorld == null)
                {
                    Log.Error($"Error ClientServerBootstrap.CreateServerWorld(\"ServerWorld\")", "WorldManager", this);
                    return false;
                }
            }

            if (_role == Role.ServerAndClient || _role == Role.Client)
            {
                ClientWorld = ClientServerBootstrap.CreateClientWorld("Natrium Client");
                DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(ClientWorld, DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.ClientSimulation));
                if (ClientWorld == null)
                {
                    Log.Error($"Error ClientServerBootstrap.CreateClientWorld(\"ClientWorld\");", "WorldManager", this);
                    return false;
                }
            }

            if (ServerWorld != null)
            {
                World.DefaultGameObjectInjectionWorld = ServerWorld;
            }
            else if (ClientWorld != null)
            {
                World.DefaultGameObjectInjectionWorld = ClientWorld;
            }

            return true;
        }

        private void UpdateCurrentRole()
        {
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
        }
    }
}