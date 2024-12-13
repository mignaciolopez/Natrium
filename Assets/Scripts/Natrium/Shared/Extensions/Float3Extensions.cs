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
        
        public static void RotateTowards(this ref quaternion quaternion, float3 currentPosition, float3 targetPosition, float maxDegreesDelta)
        {
            // Normalize directions
            var currentDirection = math.normalize(math.forward(quaternion));
            var targetDirection = math.normalize(targetPosition - currentPosition);

            // Calculate the quaternion rotation between the two directions
            quaternion targetRotation = quaternion.LookRotationSafe(targetDirection, math.up());

            // Calculate the current rotation
            quaternion currentRotation = quaternion.LookRotationSafe(currentDirection, math.up());

            // Spherically interpolate between the current rotation and the target rotation
            quaternion = math.slerp(currentRotation, targetRotation, maxDegreesDelta / 360f);
        }
    }
}