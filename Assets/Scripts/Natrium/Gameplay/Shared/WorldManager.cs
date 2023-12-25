using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

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
            _role = GetCurrentRole();
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
                ServerWorld = ClientServerBootstrap.CreateServerWorld("ServerWorld");
                if (ServerWorld == null)
                {
                    Debug.LogError($"Error ClientServerBootstrap.CreateServerWorld(\"ServerWorld\")");
                    return false;
                }
            }

            if (_role == Role.ServerAndClient || _role == Role.Client)
            {
                ClientWorld = ClientServerBootstrap.CreateClientWorld("ClientWorld");
                if (ClientWorld == null)
                {
                    Debug.LogError($"Error ClientServerBootstrap.CreateClientWorld(\"ClientWorld\");");
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

        private Role GetCurrentRole()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsServer:
                case RuntimePlatform.OSXServer:
                case RuntimePlatform.LinuxServer:
                    {
                        return Role.Server;
                    }
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.LinuxEditor:
                    {
                        return Role.ServerAndClient;
                    }
                default:
                    {
                        return Role.Client;
                    }
            }
        }
    }
}