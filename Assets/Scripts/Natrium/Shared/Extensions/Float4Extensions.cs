using Unity.Mathematics;
using UnityEngine;

namespace Natrium.Shared.Extensions
{
    public static class Float4Extensions
    {
        /// <summary>
        /// Converts a UnityEngine.Color to a float4.
        /// </summary>
        public static float4 ToFloat4(this Color color)
        {
            return new float4(color.r, color.g, color.b, color.a);
        }

        public static Color ToColor(this float4 value)
        {
            return new Color(value.x, value.y, value.z, value.w);
        }
    }
}