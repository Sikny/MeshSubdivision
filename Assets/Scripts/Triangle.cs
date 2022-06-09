using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

namespace Geometry {
    [Serializable]
    public class Triangle {
        public Edge e1, e2, e3;

        public int[] Points {
            get {
                var s1 = e1.s1;
                var s2 = s1 == e2.s1 ? e2.s2 : e2.s1;
                var s3 = s2 == e3.s1 || s1 == e3.s1 ? (s1 == e3.s2 || s2 == e3.s2 ? e1.s2 : e3.s2) : e3.s1;
                return new[] { s1, s2, s3 };
            }
        }

        public Edge[] Edges => new[] {
            e1, e2, e3
        };

        public Triangle(int a, int b, int c) {
            e1 = new Edge(a, b);
            e2 = new Edge(b, c);
            e3 = new Edge(c, a);
        }

        public Triangle(Edge[] edges) {
            Set(edges);
        }

        public void Set(Edge[] edges) {
            e1 = edges[0];
            e2 = edges[1];
            e3 = edges[2];
        }

        public bool Contains(Edge e) {
            return e1.Equals(e) || e2.Equals(e) || e3.Equals(e);
        }

        public bool Contains(int point) {
            return e1.s1 == point || e1.s2 == point || e2.s1 == point || e2.s2 == point || e3.s1 == point || e3.s2 == point;
        }

        public static List<Triangle> ListFromIndices(int[] trianglesIndices) {
            var triangles = new List<Triangle>();
            int triCount = trianglesIndices.Length / 3;

            for (int i = 0; i < triCount; ++i) {
                int tri = i * 3;
                var edges = Edge.ListFromIndices(trianglesIndices.SubArray(tri, 3));
                triangles.Add(new Triangle(edges.ToArray()));
            }

            return triangles;
        }

        public Vector3 Center(Vector3[] vertices) {
            var pts = Points;
            return (vertices[pts[0]] + vertices[pts[1]] + vertices[pts[2]]) / 3f;
        }

        public static Vector3 EdgePoint(Edge e, List<Triangle> faces, Vector3[] vertices) {
            return (vertices[e.s1] + vertices[e.s2] + faces[0].Center(vertices) +
                    faces[1].Center(vertices)) / 4f;
        }

        public static void MakeUniqueVertices(ref Vector3[] vertices, ref int[] indices) {
            const float tolerance = 0.001f;
            
            var knownIndices = new List<int>();
            for (var i = 0; i < indices.Length; ++i) {
                var index = indices[i];
                if (!knownIndices.Contains(index)) {
                    var position = vertices[index];

                    for (int i2 = 0; i2 < knownIndices.Count; ++i2) {
                        var knownIndex = knownIndices[i2];
                        if ((position - vertices[knownIndex]).sqrMagnitude < tolerance) {
                            index = indices[i] = knownIndex;
                            break;
                        }
                    }

                    knownIndices.Add(index);
                }
            }

            int maxIndex = indices.Max();
            Array.Resize(ref vertices, maxIndex + 1);
        }

        public override bool Equals(object obj) {
            var otherTri = obj as Triangle;
            if (otherTri == null) return false;
            var pts = Points;
            var otherPts = otherTri.Points;
            for (int i = pts.Length - 1; i >= 0; --i) {
                if (!otherPts.Contains(pts[i])) return false;
            }
            return true;
        }
        
        public override int GetHashCode() {
            return e1.GetHashCode() + e2.GetHashCode() + e3.GetHashCode();
        }
    }

    [Serializable]
    public class Edge {
        public int s1, s2;

        public Edge(int a, int b) {
            Set(a, b);
        }

        public override string ToString() {
            return "(" + s1 + ", " + s2 + ")";
        }

        public override bool Equals(object e2) {
            var otherEdge = e2 as Edge;
            if (otherEdge == null) return false;
            bool result = Equals(otherEdge.s1, otherEdge.s2);
            return result;
        }

        protected bool Equals(Edge other) {
            return s1 == other.s1 && s2 == other.s2;
        }

        public override int GetHashCode() {
            return s1.GetHashCode() + s2.GetHashCode();
        }

        public bool Equals(int a, int b) {
            return s1 == a && s2 == b || s2 == a && s1 == b;
        }

        public void Set(int a, int b) {
            s1 = a;
            s2 = b;
        }

        public static List<Edge> ListFromIndices(int[] indices) {
            var edges = new List<Edge>();
            int triCount = indices.Length / 3;

            for (int i = 0; i < triCount; ++i) {
                int tri = i * 3;
                var e1 = new Edge(indices[tri], indices[tri + 1]);
                var e2 = new Edge(indices[tri + 1], indices[tri + 2]);
                var e3 = new Edge(indices[tri + 2], indices[tri]);
                
                if(!edges.Contains(e1)) edges.Add(e1);
                if(!edges.Contains(e2)) edges.Add(e2);
                if(!edges.Contains(e3)) edges.Add(e3);
            }

            return edges;
        }
        
        public Vector3 Center(Vector3[] vertices) {
            return (vertices[s1] + vertices[s2]) / 2f;
        }
    }
}