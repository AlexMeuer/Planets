using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CubeSphere : MonoBehaviour {
    private static int
        SetQuad(IList<int> triangles, int i, int v00, int v10, int v01, int v11)
    {
        triangles[i] = v00;
        triangles[i + 1] = triangles[i + 4] = v01;
        triangles[i + 2] = triangles[i + 3] = v10;
        triangles[i + 5] = v11;
        return i + 6;
    }

    public int gridSize;
    public float radius = 1f;

    private Mesh _mesh;
    private Vector3[] _vertices;
    private Vector3[] _normals;
    private Color32[] _cubeUV;

    private void Awake ()
    {
        Generate();
    }

    private void Generate()
    {
        GetComponent<MeshFilter>().mesh = _mesh = new Mesh();
        _mesh.name = "Procedural Sphere";
        CreateVertices();
        CreateTriangles();
        CreateColliders();
    }

    private void CreateVertices() {

    const int cornerVertices = 8;
        var edgeVertices = (gridSize + gridSize + gridSize - 3) * 4;
        var faceVertices = (
            (gridSize - 1) * (gridSize - 1) +
            (gridSize - 1) * (gridSize - 1) +
            (gridSize - 1) * (gridSize - 1)) * 2;
        _vertices = new Vector3[cornerVertices + edgeVertices + faceVertices];
        _normals = new Vector3[_vertices.Length];
        _cubeUV = new Color32[_vertices.Length];

        var v = 0;
        for (var y = 0; y <= gridSize; y++)
        {
            for (var x = 0; x <= gridSize; x++)
            {
                SetVertex(v++, x, y, 0);
            }

            for (var z = 1; z <= gridSize; z++)
            {
                SetVertex(v++, gridSize, y, z);
            }

            for (var x = gridSize - 1; x >= 0; x--)
            {
                SetVertex(v++, x, y, gridSize);
            }

            for (var z = gridSize - 1; z > 0; z--)
            {
                SetVertex(v++, 0, y, z);
            }
        }
        for (var z = 1; z < gridSize; z++) {
            for (var x = 1; x < gridSize; x++) {
                SetVertex(v++, x, gridSize, z);
            }
        }
        for (var z = 1; z < gridSize; z++) {
            for (var x = 1; x < gridSize; x++) {
                SetVertex(v++, x, 0, z);
            }
        }

        _mesh.vertices = _vertices;
        _mesh.normals = _normals;
        _mesh.colors32 = _cubeUV;
    }

    private void SetVertex (int i, int x, int y, int z)
    {
        var v = new Vector3(x, y, z) * 2f / gridSize - Vector3.one;
        var x2 = v.x * v.x;
        var y2 = v.y * v.y;
        var z2 = v.z * v.z;
        var s = new Vector3
        {
            x = v.x * Mathf.Sqrt(1f - y2 / 2f - z2 / 2f + y2 * z2 / 3f),
            y = v.y * Mathf.Sqrt(1f - x2 / 2f - z2 / 2f + x2 * z2 / 3f),
            z = v.z * Mathf.Sqrt(1f - x2 / 2f - y2 / 2f + x2 * y2 / 3f)
        };
        _normals[i] = s;
        _vertices[i] = _normals[i] * radius;
        _cubeUV[i] = new Color32((byte)x, (byte)y, (byte)z, 0);
    }

    private void CreateTriangles()
    {
        var quads = (gridSize * gridSize + gridSize * gridSize + gridSize * gridSize) * 2;
        var trianglesZ = new int[(gridSize * gridSize) * 12];
        var trianglesX = new int[(gridSize * gridSize) * 12];
        var trianglesY = new int[(gridSize * gridSize) * 12];
        var ring = (gridSize + gridSize) * 2;
        int tZ = 0, tX = 0, tY = 0, v = 0;

        for (var y = 0; y < gridSize; y++, v++)
        {
            for (var q = 0; q < gridSize; q++, v++) {
                tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
            }
            for (var q = 0; q < gridSize; q++, v++) {
                tX = SetQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
            }
            for (var q = 0; q < gridSize; q++, v++) {
                tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
            }
            for (var q = 0; q < gridSize - 1; q++, v++) {
                tX = SetQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
            }
            tX = SetQuad(trianglesX, tX, v, v - ring + 1, v + ring, v + 1);
        }
        tY = CreateTopFace(trianglesY, tY, ring);
        tY = CreateBottomFace(trianglesY, tY, ring);
        _mesh.subMeshCount = 3;
        _mesh.SetTriangles(trianglesZ, 0);
        _mesh.SetTriangles(trianglesX, 1);
        _mesh.SetTriangles(trianglesY, 2);
    }

    private int CreateTopFace(int[] triangles, int t, int ring)
    {
        var v = ring * gridSize;
        for (var x = 0; x < gridSize - 1; x++, v++)
        {
            t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + ring);
        }
        t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + 2);

        var vMin = ring * (gridSize + 1) - 1;
        var vMid = vMin + 1;
        var vMax = v + 2;
        for (var z = 1; z < gridSize - 1; z++, vMin--, vMid++, vMax++)
        {
            t = SetQuad(triangles, t, vMin, vMid, vMin - 1, vMid + gridSize - 1);
            for (var x = 1; x < gridSize - 1; x++, vMid++)
            {
                t = SetQuad(
                    triangles, t,
                    vMid, vMid + 1, vMid + gridSize - 1, vMid + gridSize);
            }

            t = SetQuad(triangles, t, vMid, vMax, vMid + gridSize - 1, vMax + 1);
        }
        var vTop = vMin - 2;
        t = SetQuad(triangles, t, vMin, vMid, vTop + 1, vTop);
        for (var x = 1; x < gridSize - 1; x++, vTop--, vMid++) {
            t = SetQuad(triangles, t, vMid, vMid + 1, vTop, vTop - 1);
        }
        t = SetQuad(triangles, t, vMid, vTop - 2, vTop, vTop - 1);
        return t;
    }

    private int CreateBottomFace (int[] triangles, int t, int ring) {
        var v = 1;
        var vMid = _vertices.Length - (gridSize - 1) * (gridSize - 1);
        t = SetQuad(triangles, t, ring - 1, vMid, 0, 1);
        for (var x = 1; x < gridSize - 1; x++, v++, vMid++) {
            t = SetQuad(triangles, t, vMid, vMid + 1, v, v + 1);
        }
        t = SetQuad(triangles, t, vMid, v + 2, v, v + 1);

        var vMin = ring - 2;
        vMid -= gridSize - 2;
        var vMax = v + 2;

        for (var z = 1; z < gridSize - 1; z++, vMin--, vMid++, vMax++) {
            t = SetQuad(triangles, t, vMin, vMid + gridSize - 1, vMin + 1, vMid);
            for (var x = 1; x < gridSize - 1; x++, vMid++) {
                t = SetQuad(
                    triangles, t,
                    vMid + gridSize - 1, vMid + gridSize, vMid, vMid + 1);
            }
            t = SetQuad(triangles, t, vMid + gridSize - 1, vMax + 1, vMid, vMax);
        }

        var vTop = vMin - 1;
        t = SetQuad(triangles, t, vTop + 1, vTop, vTop + 2, vMid);
        for (var x = 1; x < gridSize - 1; x++, vTop--, vMid++) {
            t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vMid + 1);
        }
        t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vTop - 2);

        return t;
    }

    private void CreateColliders ()
    {
        gameObject.AddComponent<SphereCollider>();
    }

    private void OnDrawGizmos () {
        if (_vertices == null) {
            return;
        }

        var root = transform.position;
        for (var i = 0; i < _vertices.Length; i++)
        {
            var v = _vertices[i];
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(root + _vertices[i], 0.1f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(root + _vertices[i], _normals[i]);
        }
    }
}
