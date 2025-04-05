using Unity.Mathematics;

namespace CEG.Shared.Extensions
{
    public static class QuaternionExtensions
    {
        public static void RotateTowards(this ref quaternion quaternion, float3 targetDirection, float maxDegreesDelta)
        {
            // Calculate the quaternion rotation between the two directions
            quaternion targetRotation = quaternion.LookRotationSafe(targetDirection, math.up());

            // Calculate the current rotation
            var currentDirection = math.normalize(math.forward(quaternion));
            quaternion currentRotation = quaternion.LookRotationSafe(currentDirection, math.up());

            // Spherically interpolate between the current rotation and the target rotation
            quaternion = math.slerp(currentRotation, targetRotation, maxDegreesDelta / 360f);
        }
    }
}