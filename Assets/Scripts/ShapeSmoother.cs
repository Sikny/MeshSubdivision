using GeometrySmoothing;
using UnityEngine;
using UnityExtendedEditor.Attributes;

public class ShapeSmoother : MonoBehaviour {
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private SmoothMode smoothMode;
    [SerializeField] private bool drawVerticesGizmos;

    [Button]
    private void SmoothShape() {
        Mesh mesh = meshFilter.sharedMesh;
        switch (smoothMode) {
            case SmoothMode.CatmullClark:
                meshFilter.sharedMesh = Subdivisions.CatmullClark(mesh, transform);
                break;
            case SmoothMode.Loop:
                meshFilter.sharedMesh = Subdivisions.Loop(mesh);
                break;
            case SmoothMode.Root3Kobbelt:
                meshFilter.sharedMesh = Subdivisions.Root3Kobbelt(mesh);
                break;
        }
    }

    private void OnDrawGizmos() {
        if (meshFilter.sharedMesh == null) return;

        if (drawVerticesGizmos) {
            Gizmos.color = Color.red;

            var vertices = meshFilter.sharedMesh.vertices;
            foreach (var vec in vertices) {
                Gizmos.DrawSphere(transform.TransformPoint(vec), 0.05f);
            }
        }
    }
}