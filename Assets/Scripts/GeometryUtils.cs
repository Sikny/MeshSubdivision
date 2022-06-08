using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Geometry {
    public static class GeometryUtils {
        public static Vector3 Average(this Vector3[] vec) {
            Vector3 result = Vector3.zero;

            for (int i = vec.Length - 1; i >= 0; --i) {
                result += vec[i];
            }

            return result / vec.Length;
        }
    }
}