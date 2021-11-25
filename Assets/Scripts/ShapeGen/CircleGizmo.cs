using UnityEngine;

namespace ShapeGen
{
    public class CircleGizmo : MonoBehaviour
    {
        private static void ShowPoint(float x, float y)
        {
            var square = new Vector2(x, y);
            var circle = new Vector2
            {
                x = square.x * Mathf.Sqrt(1f - square.y * square.y * 0.5f),
                y = square.y * Mathf.Sqrt(1f - square.x * square.x * 0.5f)
            };

            Gizmos.color = Color.black;
            Gizmos.DrawSphere(square, 0.025f);

            Gizmos.color = Color.white;
            Gizmos.DrawSphere(circle, 0.025f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(square, circle);

            Gizmos.color = Color.gray;
            Gizmos.DrawLine(circle, Vector2.zero);
        }

        public int resolution = 10;

        private void OnDrawGizmosSelected()
        {
            var step = 2f / resolution;
            for (var i = 0; i <= resolution; i++)
            {
                ShowPoint(i * step - 1f, -1f);
                ShowPoint(i * step - 1f, 1f);
            }
            for (var i = 1; i < resolution; i++) {
                ShowPoint(-1f, i * step - 1f);
                ShowPoint(1f, i * step - 1f);
            }
        }
    }
}
