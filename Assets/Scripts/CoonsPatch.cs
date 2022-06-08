using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityExtendedEditor.Attributes;

// ReSharper disable once CheckNamespace
namespace GeometrySmoothing {
    public class CoonsPatch : MonoBehaviour {
        [SerializeField] private Curve[] curves;

        [Button]
        private void Generate() {
            Assert.AreEqual(curves.Length, 4, "Curves count must be 4");
            foreach (var curve in curves) {
                curve.SmoothCurve();
            }
            
            // todo
        }
    }
}