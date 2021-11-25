using UnityEngine;

namespace ShapeGen
{
    [RequireComponent(typeof(MeshFilter))]
    public class MeshDeformer : MonoBehaviour
    {
        public float springForce = 20f;
        public float damping = 5f;
        public float uniformScale = 1f;

        private Mesh _deformingMesh;
        private Vector3[] _originalVertices, _displacedVertices, _vertexVelocities;

        private void Start()
        {
            _deformingMesh = GetComponent<MeshFilter>().mesh;
            _originalVertices = _deformingMesh.vertices;
            _displacedVertices = new Vector3[_originalVertices.Length];
            for (var i = 0; i < _originalVertices.Length; i++) {
                _displacedVertices[i] = _originalVertices[i];
            }
            _vertexVelocities = new Vector3[_originalVertices.Length];
        }

        private void Update()
        {
            uniformScale = transform.localScale.x;
            for (var i = 0; i < _displacedVertices.Length; i++) {
                UpdateVertex(i);
            }
            _deformingMesh.vertices = _displacedVertices;
            _deformingMesh.RecalculateNormals();
        }

        private void UpdateVertex (int i) {
            var velocity = _vertexVelocities[i];
            var displacement = _displacedVertices[i] - _originalVertices[i];
            displacement *= uniformScale;
            velocity -= displacement * springForce * Time.deltaTime;
            velocity *= 1f - damping * Time.deltaTime;
            _vertexVelocities[i] = velocity;
            _displacedVertices[i] += velocity * Time.deltaTime;
        }

        public void AddDeformingForce(Vector3 point, float force)
        {
            point = transform.InverseTransformPoint(point);
            for (var i = 0; i < _displacedVertices.Length; i++)
            {
                AddForceToVertex(i, point, force);
            }
        }

        private void AddForceToVertex(int i, Vector3 point, float force)
        {
            var pointToVertex = _displacedVertices[i] - point;
            pointToVertex *= uniformScale;
            var attenuatedForce = force / (1f + pointToVertex.sqrMagnitude);
            var velocity = attenuatedForce * Time.deltaTime;
            _vertexVelocities[i] += pointToVertex.normalized * velocity;
        }
    }
}
