using System.Collections.Generic;
using System.Linq;
using Geometry;
using UnityEngine;

public static class Subdivisions {
    private const float Tolerance = 0.001f;

    private static Vector3 Sum(this IEnumerable<Vector3> points) {
        return points.Aggregate(Vector3.zero, (current, pt) => current + pt);
    }

    public static Mesh CatmullClark(Mesh mesh, Transform transform) {
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
        foreach (var face in faces) {
            var center = face.Center(vertices);
            int centerIndex = verticesOut.FindIndex(vec => (vec - center).sqrMagnitude < Tolerance);

            Edge prevEdge, edge;
            int prevEdgeInd, edgeInd, pointIndex, n, transformedIndex;
            Triangle[] touchingFaces;
            Vector3 vert, f, r, transformedVert;
            Edge[] touchingEdges;
            for (var index = face.Edges.Length - 1; index > 0; --index) {
                prevEdge = face.Edges[index - 1];
                edge = face.Edges[index];

                prevEdgeInd = verticesOut.FindIndex(vec3 => (vec3 - Triangle.EdgePoint(prevEdge,
                    faces.FindAll(tri => tri.Contains(prevEdge)), vertices)).sqrMagnitude < Tolerance);
                edgeInd = verticesOut.FindIndex(vec3 => (vec3 - Triangle.EdgePoint(edge,
                    faces.FindAll(tri => tri.Contains(edge)), vertices)).sqrMagnitude < Tolerance);
                
                trianglesOut.Add(centerIndex);
                trianglesOut.Add(prevEdgeInd);
                trianglesOut.Add(edgeInd);

                pointIndex = edge.s1;
                if (pointIndex != prevEdge.s1 && pointIndex != prevEdge.s2) {
                    pointIndex = edge.s2;
                }
                
                // f : average of all n recently created face points for faces touching vert
                touchingFaces = faces.Where(f2 => f2.Contains(pointIndex)).ToArray();
                vert = vertices[pointIndex];
                f = touchingFaces.Select(tri => tri.Center(vertices)).ToArray().Average();
                n = touchingFaces.Length;

                // r : average of all n edge midpoints for original edges touching vert
                touchingEdges = edges.Where(e => e.s1 == pointIndex || e.s2 == pointIndex).ToArray();
                r = touchingEdges.Select(e => e.Center(vertices)).ToArray().Average();

                transformedVert = (f + 2f * r + (n - 3f) * vert) / n;
                transformedIndex = verticesOut.Count;
                verticesOut.Add(transformedVert);
                
                trianglesOut.Add(transformedIndex);
                trianglesOut.Add(edgeInd);
                trianglesOut.Add(prevEdgeInd);
            }
            
            // link first and last
            prevEdge = face.Edges[0];
            edge = face.Edges[^1];

            prevEdgeInd = verticesOut.FindIndex(vec3 => (vec3 - Triangle.EdgePoint(prevEdge,
                faces.FindAll(tri => tri.Contains(prevEdge)), vertices)).sqrMagnitude < Tolerance);
            edgeInd = verticesOut.FindIndex(vec3 => (vec3 - Triangle.EdgePoint(edge,
                faces.FindAll(tri => tri.Contains(edge)), vertices)).sqrMagnitude < Tolerance);
                
            trianglesOut.Add(centerIndex);
            trianglesOut.Add(edgeInd);
            trianglesOut.Add(prevEdgeInd);

            pointIndex = edge.s1;
            if (pointIndex != prevEdge.s1 && pointIndex != prevEdge.s2) {
                pointIndex = edge.s2;
            }
                
            // f : average of all n recently created face points for faces touching vert
            touchingFaces = faces.Where(f2 => f2.Contains(pointIndex)).ToArray();
            vert = vertices[pointIndex];
            f = touchingFaces.Select(tri => tri.Center(vertices)).ToArray().Average();
            n = touchingFaces.Length;

            // r : average of all n edge midpoints for original edges touching vert
            touchingEdges = edges.Where(e => e.s1 == pointIndex || e.s2 == pointIndex).ToArray();
            r = touchingEdges.Select(e => e.Center(vertices)).ToArray().Average();

            transformedVert = (f + 2f * r + (n - 3f) * vert) / n;
            transformedIndex = verticesOut.Count;
            verticesOut.Add(transformedVert);
                
            trianglesOut.Add(transformedIndex);
            trianglesOut.Add(prevEdgeInd);
            trianglesOut.Add(edgeInd);
        }

        result.vertices = verticesOut.ToArray();
        result.triangles = trianglesOut.ToArray();

        return result;
    }


    public static Mesh Loop(Mesh mesh) {
        Mesh result = new Mesh();

        var vertices = mesh.vertices;
        var indices = mesh.triangles;


        return result;
    }

    public static Mesh Root3Kobbelt(Mesh mesh) {
        Mesh result = new Mesh();

        var verts = mesh.vertices;
        var indices = mesh.triangles;

        Triangle.MakeUniqueVertices(ref verts, ref indices);

        var vertices = verts.ToList();

        var triangles = Triangle.ListFromIndices(indices);
        var trianglesOut = new List<Triangle>();
        for (int i = 0; i < triangles.Count; ++i) {
            var triangle = triangles[i];
            var pts = triangle.Points;
            var center = triangle.Center(vertices.ToArray());
            int cent = vertices.Count;

            int ind0 = pts[0];
            int ind1 = pts[1];
            int ind2 = pts[2];
            vertices.Add(center);
            trianglesOut.Add(new Triangle(ind0, ind1, cent));
            trianglesOut.Add(new Triangle(ind2, ind0, cent));
            trianglesOut.Add(new Triangle(ind1, ind2, cent));
        }

        result.vertices = vertices.ToArray();
        result.triangles = trianglesOut.Select(tri => tri.Points).SelectMany(pt => pt).ToArray();

        return result;
    }
}