using System.Collections.Generic;
using System.Linq;
using Geometry;
using Unity.VisualScripting;
using UnityEngine;
using Edge = Geometry.Edge;

public static class Subdivisions {
    private const float Tolerance = 0.001f;

    private static Vector3 Sum(this IEnumerable<Vector3> points) {
        return points.Aggregate(Vector3.zero, (current, pt) => current + pt);
    }

    public static Mesh CatmullClark(Mesh mesh) {
        // 1 - get list of faces
        var vertices = mesh.vertices;
        var indices = mesh.triangles;

        Mesh result = new Mesh();

        Triangle.MakeUniqueVertices(ref vertices, ref indices);

        var faces = Triangle.ListFromIndices(indices);
        var edges = Edge.ListFromIndices(indices);

        var verticesOut = new List<Vector3>();
        var trianglesOut = new List<int>();

        // 1 - compute faces centers
        foreach (var face in faces) {
            var center = face.Center(vertices);
            verticesOut.Add(center);
        }

        // 2 - Compute edge points - center of face points & edge corners
        foreach (var edge in edges) {
            var edgeFaces = faces.FindAll(tri => tri.Contains(edge));
            var edgePoint = Triangle.EdgePoint(edge, edgeFaces, vertices);
            verticesOut.Add(edgePoint);
        }
        
        // 3 - Link faces centers to edge points and transform original vertices
        void CreateTriangles(Edge prevEdge, Edge edge, int centerIndex) {
            int prevEdgeInd = verticesOut.FindIndex(vec3 => (vec3 - Triangle.EdgePoint(prevEdge,
                faces.FindAll(tri => tri.Contains(prevEdge)), vertices)).sqrMagnitude < Tolerance);
            int edgeInd = verticesOut.FindIndex(vec3 => (vec3 - Triangle.EdgePoint(edge,
                faces.FindAll(tri => tri.Contains(edge)), vertices)).sqrMagnitude < Tolerance);
            
            trianglesOut.Add(centerIndex);
            trianglesOut.Add(prevEdgeInd);
            trianglesOut.Add(edgeInd);

            var pointIndex = edge.s1;
            if (pointIndex != prevEdge.s1 && pointIndex != prevEdge.s2) {
                pointIndex = edge.s2;
            }
                
            // f : average of all n recently created face points for faces touching vert
            var touchingFaces = faces.Where(f2 => f2.Contains(pointIndex)).ToArray();
            var vert = vertices[pointIndex];
            var f = touchingFaces.Select(tri => tri.Center(vertices)).ToArray().Average();
            var n = touchingFaces.Length;

            // r : average of all n edge midpoints for original edges touching vert
            var touchingEdges = edges.Where(e => e.s1 == pointIndex || e.s2 == pointIndex).ToArray();
            var r = touchingEdges.Select(e => e.Center(vertices)).ToArray().Average();

            var transformedVert = (f + 2f * r + (n - 3f) * vert) / n;
            var transformedIndex = verticesOut.Count;
            verticesOut.Add(transformedVert);
                
            trianglesOut.Add(transformedIndex);
            trianglesOut.Add(edgeInd);
            trianglesOut.Add(prevEdgeInd);
        }
        
        foreach (var face in faces) {
            var center = face.Center(vertices);
            int centerIndex = verticesOut.FindIndex(vec => (vec - center).sqrMagnitude < Tolerance);

            for (var index = face.Edges.Length - 1; index > 0; --index) {
                CreateTriangles(face.Edges[index - 1], face.Edges[index], centerIndex);
            }
            
            // link first and last
            CreateTriangles(face.Edges[^1], face.Edges[0], centerIndex);
        }

        result.vertices = verticesOut.ToArray();
        result.triangles = trianglesOut.ToArray();

        return result;
    }


    public static Mesh Loop(Mesh mesh) {
        Mesh result = new Mesh();

        var vertices = mesh.vertices;
        var indices = mesh.triangles;

        Triangle.MakeUniqueVertices(ref vertices, ref indices);

        var faces = Triangle.ListFromIndices(indices);
        var edges = Edge.ListFromIndices(indices);

        var verticesOut = new List<Vector3>();
        var indicesOut = new List<int>();
        
        // 1 - Compute a new edge point for each edge
        var edgePoints = new Dictionary<Edge, Vector3>();
        for (int i = edges.Count - 1; i >= 0; --i) {
            var e = edges[i];
            var touchingFaces = faces.Where(f2 => f2.Contains(e)).ToArray();
            var vLeft = touchingFaces[0].Points.Except(new []{ e.s1, e.s2 }).First();
            var vRight = touchingFaces[1].Points.Except(new []{ e.s1, e.s2 }).First();
            var edgePoint = 3 / 8f * (vertices[e.s1] + vertices[e.s2]) + 1 / 8f * (vertices[vLeft] + vertices[vRight]);
            edgePoints.Add(e, edgePoint);
        }
        
        // 2 - Compute new vertex point for each original vertex
        for (int i = 0; i < vertices.Length; ++i) {
            var vertIndex = i;
            var vert = vertices[vertIndex];
            var touchingVertsIndices = edges.Where(e => e.s1 == vertIndex || e.s2 == vertIndex)
                .Select(e => e.s1 == vertIndex ? e.s2 : e.s1).ToArray();
            var n = touchingVertsIndices.Length;
            var alpha = n > 3
                ? 1f / n * (5 / 8f - Mathf.Pow(3 / 8f + 1 / 4f * Mathf.Cos(2 * Mathf.PI / n), 2))
                : 3 / 16f;
            var newVertex = (1 - n * alpha) * vert + alpha * Sum(touchingVertsIndices.Select(ind => vertices[ind]));
            verticesOut.Add(newVertex);
        }
        
        // 3 - Construct new triangles
        foreach (var face in faces) {
            var vertIndices = face.Points;
            var v1 = vertIndices[0];
            var v2 = vertIndices[1];
            var v3 = vertIndices[2];

            var e1 = edgePoints[new Edge(v2, v3)];
            var e2 = edgePoints[new Edge(v1, v3)];
            var e3 = edgePoints[new Edge(v1, v2)];
            
            var e1Ind = verticesOut.Count;
            verticesOut.Add(e1);
            var e2Ind = verticesOut.Count;
            verticesOut.Add(e2);
            var e3Ind = verticesOut.Count;
            verticesOut.Add(e3);
            
            
            indicesOut.Add(v1);
            indicesOut.Add(e3Ind);
            indicesOut.Add(e2Ind);
            
            indicesOut.Add(v2);
            indicesOut.Add(e1Ind);
            indicesOut.Add(e3Ind);
            
            indicesOut.Add(v3);
            indicesOut.Add(e2Ind);
            indicesOut.Add(e1Ind);

            indicesOut.Add(e1Ind);
            indicesOut.Add(e2Ind);
            indicesOut.Add(e3Ind);
        }

        result.vertices = verticesOut.ToArray();
        result.triangles = indicesOut.ToArray();

        return result;
    }

    public static Mesh Root3Kobbelt(Mesh mesh) {
        Mesh result = new Mesh();

        var vertices = mesh.vertices;
        var indices = mesh.triangles;

        Triangle.MakeUniqueVertices(ref vertices, ref indices);

        var faces = Triangle.ListFromIndices(indices);
        var edges = Edge.ListFromIndices(indices);

        var verticesOut = new List<Vector3>();
        var indicesOut = new List<int>();

        // Compute perturbed vertices
        for (int i = 0; i < vertices.Length; ++i) {
            int vertIndex = i;
            var vert = vertices[vertIndex];
            var touchingVertsIndices = edges.Where(e => e.s1 == vertIndex || e.s2 == vertIndex)
                .Select(e => e.s1 == vertIndex ? e.s2 : e.s1).ToArray();
            var n = touchingVertsIndices.Length;
            var alpha = 1 / 9f * (4 - 2f * Mathf.Cos(2f * Mathf.PI / n));
            var transformedVertex = (1 - alpha) * vert + alpha / n * Sum(touchingVertsIndices.Select(ind => vertices[ind]));
            
            verticesOut.Add(transformedVertex);
        }

        var centersIndices = new Dictionary<Triangle, int>();
        var newTriangles = new List<Triangle>();
        // build triangles from faces centers
        foreach (var face in faces) {
            var vertIndices = face.Points;
            var center = face.Center(vertices);

            var v1 = vertIndices[0];
            var v2 = vertIndices[1];
            var v3 = vertIndices[2];

            var centerInd = verticesOut.Count;
            verticesOut.Add(center);

            newTriangles.Add(new Triangle(v1, v2, centerInd));
            
            newTriangles.Add(new Triangle(v1, centerInd, v3));
            
            newTriangles.Add(new Triangle(v2, v3, centerInd));

            centersIndices.Add(face, centerInd);
        }
        
        // flipping
        for (var index = 0; index < edges.Count; index++) {
            var edge = edges[index];
            var edgeFaces = faces.FindAll(tri => tri.Contains(edge));
            var center1 = centersIndices[edgeFaces[0]];
            var center2 = centersIndices[edgeFaces[1]];

            int indexVert = 0;
            newTriangles.ForEach(tri => {
                if (tri.Contains(edge)) {
                    tri.Set(new [] {
                        new Edge(center2, center1),
                        new Edge(center1, indexVert == 0 ? edge.s1 : edge.s2),
                        new Edge(indexVert == 0 ? edge.s1 : edge.s2, center2)
                    });
                    ++indexVert;
                }
            });
        }

        indicesOut.Clear();
        for (int i = 0; i < newTriangles.Count; ++i) {
            var pts = newTriangles[i].Points;
            indicesOut.Add(pts[0]);
            indicesOut.Add(pts[1]);
            indicesOut.Add(pts[2]);
        }

        result.vertices = verticesOut.ToArray();
        result.triangles = indicesOut.ToArray();

        return result;
    }
}