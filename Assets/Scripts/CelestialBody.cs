using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class CelestialBody : MonoBehaviour
{
    [Range(0, 6)]
    public int subdivisions = 0;
    public float radius = 1f;

    // Update is called once per frame
    private void Update()
    {
        GetComponent<MeshFilter>().mesh = OctahedronSphereGenerator.Generate(subdivisions, radius);
        transform.Rotate(Vector3.up, 30 * Time.deltaTime);
    }
}
