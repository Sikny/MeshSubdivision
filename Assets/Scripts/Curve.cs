using System;
using System.Collections.Generic;
using System.Linq;
using GeometrySmoothing;
using UnityEditor;
using UnityEngine;
using UnityExtendedEditor.Attributes;

public class Curve : MonoBehaviour {
    public bool loop;
    [SerializeField] private float smoothStep1 = 0.1f;
    [SerializeField] private float smoothStep2 = 0.1f;
    [SerializeField, Range(1, 10)] private int strength;
    [SerializeField] private Color pointsColor = Color.red;
    [SerializeField] private Color lineColor = Color.white;
    [SerializeField] private Color smoothedLineColor = Color.green;
    [SerializeField] private List<Transform> curvePoints = new();

    [NonSerialized] public Vector3[] SmoothedPoints;

    [Button(buttonText = "Update Points")]
    private void OnValidate() {
        curvePoints = new List<Transform>(GetComponentsInChildren<Transform>().Where(
            v => v != transform));
        if (smoothStep2 + smoothStep1 > 1) smoothStep2 = 1 - smoothStep1;

        SmoothCurve();
    }

    [Button]
    private void CleanPoints() {
        for (int i = curvePoints.Count - 1; i >= 0; --i) {
#if UNITY_EDITOR
            if (Application.isPlaying) {
#endif
                Destroy(curvePoints[i].gameObject);
#if UNITY_EDITOR
            }
            else {
                DestroyImmediate(curvePoints[i].gameObject);
            }
#endif
        }
        curvePoints.Clear();
    }

    private void OnDrawGizmos() {
        for (var index = curvePoints.Count - 1; index >= 0; --index) {
            var point = curvePoints[index];
            if (index > 0) {
                Gizmos.color = lineColor;
                Gizmos.DrawLine(point.position, curvePoints[index - 1].position);
            }

            Gizmos.color = pointsColor;
            Gizmos.DrawSphere(point.position, 0.1f);
        }

        if (loop) {
            Gizmos.color = lineColor;
            Gizmos.DrawLine(curvePoints[^1].position, curvePoints[0].position);
        }

        // smoothed curve
        if (SmoothedPoints == null) return;
        for (int i = SmoothedPoints.Length - 1; i >= 0; --i) {
            var point = SmoothedPoints[i];
            
            if (i > 0) {
                Gizmos.color = smoothedLineColor;
                Gizmos.DrawLine(point, SmoothedPoints[i - 1]);
            }
        }
    }

    [Button]
    public void SmoothCurve() {
#if UNITY_EDITOR
        Undo.RecordObject(this, "Smooth curve");
#endif
        SmoothedPoints = curvePoints.Select(p => p.position).ToArray();
        for (int i = 0; i < strength; ++i) {
            SimpleCornerCutting.SmoothCurve(this, smoothStep1, smoothStep2);
        }
    }
}