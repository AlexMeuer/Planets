using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class CelestialBody : MonoBehaviour
{
    [Range(0, 6)]
    public int subdivisions = 0;
    public float radius = 1f;

    private void Start()
    {
        var mesh = OctahedronSphereGenerator.Generate(subdivisions, radius);
        for (var i = 0; i < mesh.Vertices.Length; i++)
        {
            mesh.Vertices[i] += mesh.Normals[i] * Mathf.PerlinNoise(Mathf.Sin(i), Mathf.Cos(i));
        }

        GetComponent<MeshFilter>().mesh = mesh.Bake();
    }

    // Update is called once per frame
    private void Update()
    {
        transform.Rotate(Vector3.up, 30 * Time.deltaTime);
    }
}
