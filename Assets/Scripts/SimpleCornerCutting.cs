using System.Collections.Generic;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace GeometrySmoothing {
    public class SimpleCornerCutting {
        public static void SmoothCurve(Curve curve, float step1, float step2) {
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

                var mid1 = a + dir * step1;
                var mid2 = a + dir * (step1 + (1 - (step1 + step2)));

                pointsOut.AddRange(new[] {
                    mid1, mid2
                });
            }

            curve.SmoothedPoints = pointsOut.ToArray();
        }
    }
}