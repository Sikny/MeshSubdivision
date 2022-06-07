using System.Collections.Generic;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace GeometrySmoothing {
    public class SimpleCornerCutting {
        public static void SmoothCurve(Curve curve, float minStep = 0.01f) {
            var points = curve.SmoothedPoints;
            var edgesSrc = new List<(Vector3, Vector3)>();
            for (int i = 0; i < points.Length; ++i) {
                if (i < points.Length - 1) {
                    edgesSrc.Add((points[i], points[i + 1]));
                }
            }

            var pointsOut = new List<Vector3>();
            for (int i = 0; i < edgesSrc.Count; ++i) {
                var a = edgesSrc[i].Item1;
                var b = edgesSrc[i].Item2;

                var dir = b - a;

                var u = Random.Range(minStep, 1f);
                var v = Random.Range(minStep, 1f - u);

                var mid1 = a + dir * u;
                var mid2 = a + dir * (u + (1 - (u + v)));

                pointsOut.AddRange(new[] {
                    mid1, mid2
                });
            }

            curve.SmoothedPoints = pointsOut.ToArray();
        }
    }
}