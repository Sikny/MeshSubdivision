using GeometrySmoothing;
using TMPro;
using UnityEngine;
using UnityExtendedEditor.Attributes;

public class ShapeSmoother : MonoBehaviour {
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private SmoothMode smoothMode;
    [SerializeField] private bool drawVerticesGizmos;
    [SerializeField] private TextMeshPro demoText;

    private void OnValidate() {
        if(demoText != null) demoText.text = smoothMode.ToString();
    }

    [Button]
    private void SmoothShape() {
        Mesh mesh = meshFilter.sharedMesh;
        switch (smoothMode) {
            case SmoothMode.CatmullClark:
                mesh = Subdivisions.CatmullClark(mesh);
                break;
            case SmoothMode.Loop:
                mesh = Subdivisions.Loop(mesh);
                break;
            case SmoothMode.Kobbelt:
                mesh = Subdivisions.Root3Kobbelt(mesh);
                break;
            case SmoothMode.Butterfly:
                mesh = Subdivisions.Butterfly(mesh);
                break;
        }

        mesh.RecalculateNormals();
        meshFilter.sharedMesh = mesh;
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