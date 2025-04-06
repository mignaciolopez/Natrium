using Unity.Mathematics;

namespace CEG.Extensions
{
    public static class Float3Extensions
    {
        /// <summary>
        /// 
        /// </summary>
        public static void MoveTowards(this ref float3 current, float3 target, float maxDistanceDelta)
        {
            var x = target.x - current.x;
            var y = target.y - current.y;
            var z = target.z - current.z;
            
            var d = x * x + y * y + z * z;
            if (d == 0.0 || maxDistanceDelta >= 0.0 && d <= maxDistanceDelta * maxDistanceDelta)
            {
                current = target;
                return;
            }

            var num4 = math.sqrt(d);

            current = new float3(current.x + x / num4 * maxDistanceDelta, current.y + y / num4 * maxDistanceDelta,
                current.z + z / num4 * maxDistanceDelta);
        }
    }
}