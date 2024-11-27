using Unity.Mathematics;

namespace Natrium.Shared.Extensions
{
    public static class Float3Extensions
    {
        /// <summary>
        /// 
        /// </summary>
        public static void MoveTowards(this ref float3 current, float3 target, float maxDistanceDelta)
        {
            float num1 = target.x - current.x;
            float num2 = target.y - current.y;
            float num3 = target.z - current.z;
            float d = (float)((double)num1 * (double)num1 + (double)num2 * (double)num2 + (double)num3 * (double)num3);
            if ((double)d == 0.0 || (double)maxDistanceDelta >= 0.0 &&
                (double)d <= (double)maxDistanceDelta * (double)maxDistanceDelta)
            {
                current = target;
                return;
            }

            float num4 = (float)math.sqrt((double)d);

            current = new float3(current.x + num1 / num4 * maxDistanceDelta, current.y + num2 / num4 * maxDistanceDelta,
                current.z + num3 / num4 * maxDistanceDelta);
        }
    }
}