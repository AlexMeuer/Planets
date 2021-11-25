using UnityEngine;

namespace ShapeGen
{
    [RequireComponent(typeof(Camera))]
    public class MeshDeformerInput : MonoBehaviour
    {
        public float force = 10f;
        public float forceOffset = 0.1f;

        private void Update()
        {
            if (Input.GetMouseButton(0))
            {
                HandleInput();
            }
        }

        private void HandleInput()
        {
            var inputRay = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (!Physics.Raycast(inputRay, out hit)) return;
            var deformer = hit.collider.GetComponent<MeshDeformer>();
            if (!deformer) return;
            var point = hit.point + hit.normal * forceOffset;
            deformer.AddDeformingForce(point, force);
        }
    }
}
