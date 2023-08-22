using Unity.NetCode;
using Unity.Mathematics;

namespace Natrium
{
    public struct TouchData : IRpcCommand
    {
        public int3 tile;
    }
}