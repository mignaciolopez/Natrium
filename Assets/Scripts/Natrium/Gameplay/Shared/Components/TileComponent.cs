using Unity.Mathematics;
using Unity.NetCode;

namespace Natrium.Gameplay.Shared.Components
{
    public struct Tile : IRpcCommand
    {
        public float3 Start;
        public float3 End;
        public int NetworkIdSource;
    }
}
