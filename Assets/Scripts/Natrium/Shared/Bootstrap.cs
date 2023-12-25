using Unity.NetCode;

namespace Natrium.Shared
{
    [UnityEngine.Scripting.Preserve]
    public class Bootstrap : ClientServerBootstrap
    {
        public override bool Initialize(string defaultWorldName)
        {
            return false;
        }
    }
}
