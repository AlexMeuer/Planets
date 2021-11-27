using Scratch.Settings;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Scratch
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class Planet : MonoBehaviour
    {
        private static readonly int Radius = Shader.PropertyToID("_Radius");

        public float SpinSpeed = 1f;
        public Vector3 SpinAxis = Vector3.up;

        [Header("Sphere Generation")]

        [Range(0, 6)]
        public int subdivisions;
        [Range(1f, 10f)]
        public float radius;

        [Header("Noise")]

        public FractalNoiseSettings continents;
        public RidgeNoiseSettings mountains;
        public FractalNoiseSettings mask;

        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;

        private void OnEnable()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();

            _meshFilter.mesh = PlanetCreator.Create(subdivisions, radius, continents, mountains, mask);
            _meshRenderer.material.SetFloat(Radius, radius);
        }

        private void Update()
        {
            // _meshFilter.mesh = PlanetCreator.Create(subdivisions, radius, continents, mountains, mask);
            // _meshRenderer.material.SetFloat(Radius, radius);

            // seed += Time.deltaTime;
            transform.Rotate(SpinAxis, Time.deltaTime * SpinSpeed);
        }

        private void OnDrawGizmos()
        {
            var pos = transform.position;
            var ax = SpinAxis * Radius * 1.2f;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(pos - ax, pos + ax);
            Gizmos.color = Color.gray;
            Gizmos.DrawSphere(pos, radius);
        }
    }
}
