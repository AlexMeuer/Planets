using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Scratch
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class Planet : MonoBehaviour
    {
        public int subdivisions;
        public float radius;

        private MeshFilter _meshFilter;

        private void OnEnable()
        {
            _meshFilter = GetComponent<MeshFilter>();
        }

        private void Update()
        {
            _meshFilter.mesh = PlanetCreator.Create(subdivisions, radius);
        }
    }
}
