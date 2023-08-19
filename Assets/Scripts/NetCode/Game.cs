using System;
using Unity.Entities;
using Unity.NetCode;

namespace Natrium
{
    // Create a custom bootstrap, which enables auto-connect.
    // The bootstrap can also be used to configure other settings as well as to
    // manually decide which worlds (client and server) to create based on user input
    [UnityEngine.Scripting.Preserve]
    public class GameBootstrap : ClientServerBootstrap
    {
        public static GameBootstrap instance;

        public override bool Initialize(string defaultWorldName)
        {
            UnityEngine.Debug.Log($"GameBootstrap initializing for World: {defaultWorldName}");
            instance ??= this;

            AutoConnectPort = 7979; // Enabled auto connect
            return base.Initialize(defaultWorldName); // Use the regular bootstrap
        }
    }
}
