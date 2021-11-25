using System.Collections.Generic;
using UnityEngine;

namespace ShapeGen
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class RoundedCube : MonoBehaviour {
        private static int
            SetQuad(IList<int> triangles, int i, int v00, int v10, int v01, int v11)
        {
            triangles[i] = v00;
            triangles[i + 1] = triangles[i + 4] = v01;
            triangles[i + 2] = triangles[i + 3] = v10;
            triangles[i + 5] = v11;
            return i + 6;
        }

        public int xSize, ySize, zSize;
        public int roundness;

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
            _mesh.name = "Procedural Cube";
            CreateVertices();
            CreateTriangles();
            CreateColliders();
        }

        private void CreateVertices() {

            const int cornerVertices = 8;
            var edgeVertices = (xSize + ySize + zSize - 3) * 4;
            var faceVertices = (
                (xSize - 1) * (ySize - 1) +
                (xSize - 1) * (zSize - 1) +
                (ySize - 1) * (zSize - 1)) * 2;
            _vertices = new Vector3[cornerVertices + edgeVertices + faceVertices];
            _normals = new Vector3[_vertices.Length];
            _cubeUV = new Color32[_vertices.Length];

            var v = 0;
            for (var y = 0; y <= ySize; y++)
            {
                for (var x = 0; x <= xSize; x++)
                {
                    SetVertex(v++, x, y, 0);
                }

                for (var z = 1; z <= zSize; z++)
                {
                    SetVertex(v++, xSize, y, z);
                }

                for (var x = xSize - 1; x >= 0; x--)
                {
                    SetVertex(v++, x, y, zSize);
                }

                for (var z = zSize - 1; z > 0; z--)
                {
                    SetVertex(v++, 0, y, z);
                }
            }
            for (var z = 1; z < zSize; z++) {
                for (var x = 1; x < xSize; x++) {
                    SetVertex(v++, x, ySize, z);
                }
            }
            for (var z = 1; z < zSize; z++) {
                for (var x = 1; x < xSize; x++) {
                    SetVertex(v++, x, 0, z);
                }
            }

            _mesh.vertices = _vertices;
            _mesh.normals = _normals;
            _mesh.colors32 = _cubeUV;
        }

        private void SetVertex (int i, int x, int y, int z) {
            var inner = _vertices[i] = new Vector3(x, y, z);
            if (x < roundness) {
                inner.x = roundness;
            }
            else if (x > xSize - roundness) {
                inner.x = xSize - roundness;
            }
            if (y < roundness) {
                inner.y = roundness;
            }
            else if (y > ySize - roundness) {
                inner.y = ySize - roundness;
            }
            if (z < roundness) {
                inner.z = roundness;
            }
            else if (z > zSize - roundness) {
                inner.z = zSize - roundness;
            }

            _normals[i] = (_vertices[i] - inner).normalized;
            _vertices[i] = inner + _normals[i] * roundness;
            _cubeUV[i] = new Color32((byte)x, (byte)y, (byte)z, 0);
        }

        private void CreateTriangles()
        {
            var quads = (xSize * ySize + xSize * zSize + ySize * zSize) * 2;
            var trianglesZ = new int[(xSize * ySize) * 12];
            var trianglesX = new int[(ySize * zSize) * 12];
            var trianglesY = new int[(xSize * zSize) * 12];
            var ring = (xSize + zSize) * 2;
            int tZ = 0, tX = 0, tY = 0, v = 0;

            for (var y = 0; y < ySize; y++, v++)
            {
                for (var q = 0; q < xSize; q++, v++) {
                    tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
                }
                for (var q = 0; q < zSize; q++, v++) {
                    tX = SetQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
                }
                for (var q = 0; q < xSize; q++, v++) {
                    tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
                }
                for (var q = 0; q < zSize - 1; q++, v++) {
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
            var v = ring * ySize;
            for (var x = 0; x < xSize - 1; x++, v++)
            {
                t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + ring);
            }
            t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + 2);

            var vMin = ring * (ySize + 1) - 1;
            var vMid = vMin + 1;
            var vMax = v + 2;
            for (var z = 1; z < zSize - 1; z++, vMin--, vMid++, vMax++)
            {
                t = SetQuad(triangles, t, vMin, vMid, vMin - 1, vMid + xSize - 1);
                for (var x = 1; x < xSize - 1; x++, vMid++)
                {
                    t = SetQuad(
                        triangles, t,
                        vMid, vMid + 1, vMid + xSize - 1, vMid + xSize);
                }

                t = SetQuad(triangles, t, vMid, vMax, vMid + xSize - 1, vMax + 1);
            }
            var vTop = vMin - 2;
            t = SetQuad(triangles, t, vMin, vMid, vTop + 1, vTop);
            for (var x = 1; x < xSize - 1; x++, vTop--, vMid++) {
                t = SetQuad(triangles, t, vMid, vMid + 1, vTop, vTop - 1);
            }
            t = SetQuad(triangles, t, vMid, vTop - 2, vTop, vTop - 1);
            return t;
        }

        private int CreateBottomFace (int[] triangles, int t, int ring) {
            var v = 1;
            var vMid = _vertices.Length - (xSize - 1) * (zSize - 1);
            t = SetQuad(triangles, t, ring - 1, vMid, 0, 1);
            for (var x = 1; x < xSize - 1; x++, v++, vMid++) {
                t = SetQuad(triangles, t, vMid, vMid + 1, v, v + 1);
            }
            t = SetQuad(triangles, t, vMid, v + 2, v, v + 1);

            var vMin = ring - 2;
            vMid -= xSize - 2;
            var vMax = v + 2;

            for (var z = 1; z < zSize - 1; z++, vMin--, vMid++, vMax++) {
                t = SetQuad(triangles, t, vMin, vMid + xSize - 1, vMin + 1, vMid);
                for (var x = 1; x < xSize - 1; x++, vMid++) {
                    t = SetQuad(
                        triangles, t,
                        vMid + xSize - 1, vMid + xSize, vMid, vMid + 1);
                }
                t = SetQuad(triangles, t, vMid + xSize - 1, vMax + 1, vMid, vMax);
            }

            var vTop = vMin - 1;
            t = SetQuad(triangles, t, vTop + 1, vTop, vTop + 2, vMid);
            for (var x = 1; x < xSize - 1; x++, vTop--, vMid++) {
                t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vMid + 1);
            }
            t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vTop - 2);

            return t;
        }

        private void CreateColliders () {
            void AddBoxCollider (float x, float y, float z) {
                var c = gameObject.AddComponent<BoxCollider>();
                c.size = new Vector3(x, y, z);
            }
            void AddCapsuleCollider (int direction, float x, float y, float z) {
                var c = gameObject.AddComponent<CapsuleCollider>();
                c.center = new Vector3(x, y, z);
                c.direction = direction;
                c.radius = roundness;
                c.height = c.center[direction] * 2f;
            }

            AddBoxCollider(xSize, ySize - roundness * 2, zSize - roundness * 2);
            AddBoxCollider(xSize - roundness * 2, ySize, zSize - roundness * 2);
            AddBoxCollider(xSize - roundness * 2, ySize - roundness * 2, zSize);

            var min = Vector3.one * roundness;
            var half = new Vector3(xSize, ySize, zSize) * 0.5f;
            var max = new Vector3(xSize, ySize, zSize) - min;

            AddCapsuleCollider(0, half.x, min.y, min.z);
            AddCapsuleCollider(0, half.x, min.y, max.z);
            AddCapsuleCollider(0, half.x, max.y, min.z);
            AddCapsuleCollider(0, half.x, max.y, max.z);

            AddCapsuleCollider(1, min.x, half.y, min.z);
            AddCapsuleCollider(1, min.x, half.y, max.z);
            AddCapsuleCollider(1, max.x, half.y, min.z);
            AddCapsuleCollider(1, max.x, half.y, max.z);

            AddCapsuleCollider(2, min.x, min.y, half.z);
            AddCapsuleCollider(2, min.x, max.y, half.z);
            AddCapsuleCollider(2, max.x, min.y, half.z);
            AddCapsuleCollider(2, max.x, max.y, half.z);
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
}
