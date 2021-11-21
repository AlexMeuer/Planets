using System;
using UnityEngine;

public class MeshData
{
    public readonly string Name;
    public Vector3[] Vertices;
    public Vector3[] Normals;
    public Vector2[] UV;
    public int[] Triangles;

    public MeshData(string name)
    {
        Name = name;
    }

    public Mesh Bake() => new Mesh
    {
        name = Name,
        vertices = Vertices,
        normals = Normals,
        uv = UV,
        triangles = Triangles
    };
}

public static class OctahedronSphereGenerator
{
    private static readonly Vector3[] Directions =
    {
        Vector3.left,
        Vector3.back,
        Vector3.right,
        Vector3.forward
    };

    private const int MinSubdivisions = 0;
    private const int MaxSubdivisions = 6;

    public static MeshData Generate(int subdivisions, float radius)
    {
        if (subdivisions < MinSubdivisions)
        {
            subdivisions = MinSubdivisions;

            Debug.LogWarning($"Octahedron Sphere subdivisions increased to minimum, which is {MinSubdivisions}.");
        } else if (subdivisions > MaxSubdivisions)
        {
            subdivisions = MaxSubdivisions;

            Debug.LogWarning($"Octahedron Sphere subdivisions decreased to maximum, which is {MaxSubdivisions}.");
        }

        var resolution = 1 << subdivisions;
        var data = new MeshData($"Octahedron Sphere §{subdivisions}")
        {
            Vertices = new Vector3[(resolution + 1) * (resolution + 1) * 4 - (resolution * 2 - 1) * 3],
            Triangles = new int[(1 << (subdivisions * 2 + 3)) * 3]
        };

        GenerateOctahedron(data.Vertices, data.Triangles, resolution);
        Normalize(data.Vertices, out data.Normals);
        CreateUV(data.Vertices, out data.UV);
 
        if (Math.Abs(radius - 1f) > float.Epsilon) {
            for (var i = 0; i < data.Vertices.Length; i++) {
                data.Vertices[i] *= radius;
            }
        }

        return data;
    }

    private static void GenerateOctahedron(Vector3[] vertices, int[] triangles, int resolution)
    {
        int GenerateVertexLine(Vector3 from, Vector3 to, int steps, int v, Vector3[] vertices)
        {
            for (var i = 1; i <= steps; i++)
            {
                vertices[v++] = Vector3.Lerp(from, to, (float) i / steps);
            }
            return v;
        }

        int GenerateLowerStrip (int steps, int vTop, int vBottom, int t, int[] triangles) {
            for (var i = 1; i < steps; i++) {
                triangles[t++] = vBottom;
                triangles[t++] = vTop - 1;
                triangles[t++] = vTop;

                triangles[t++] = vBottom++;
                triangles[t++] = vTop++;
                triangles[t++] = vBottom;
            }
            triangles[t++] = vBottom;
            triangles[t++] = vTop - 1;
            triangles[t++] = vTop;
            return t;
        }

        int GenerateUpperStrip (int steps, int vTop, int vBottom, int t, int[] triangles) {
            triangles[t++] = vBottom;
            triangles[t++] = vTop - 1;
            triangles[t++] = ++vBottom;
            for (var i = 1; i <= steps; i++) {
                triangles[t++] = vTop - 1;
                triangles[t++] = vTop;
                triangles[t++] = vBottom;

                triangles[t++] = vBottom;
                triangles[t++] = vTop++;
                triangles[t++] = ++vBottom;
            }
            return t;
        }

        int v = 0, vBottom = 0, t = 0;

        for (var i = 0; i < 4; i++) {
            vertices[v++] = Vector3.down;
        }

        // Generate the bottom of the shape
        for (var i = 1; i <= resolution; i++) {

            var progress = (float)i / resolution;

            Vector3 from, to;

            vertices[v++] = to = Vector3.Lerp(Vector3.down, Vector3.forward, progress);

            foreach (var d in Directions)
            {
                from = to;

                to = Vector3.Lerp(Vector3.down, d, progress);

                t = GenerateLowerStrip(i, v, vBottom, t, triangles);

                v = GenerateVertexLine(from, to, i, v, vertices);

                vBottom += i > 1 ? i - 1 : 1;
            }

            vBottom = v - 1 - i * 4;
        }

        // Generate the top of the shape
        for (var i = resolution - 1; i >= 1; i--) {

            var progress = (float)i / resolution;

            Vector3 from, to;

            vertices[v++] = to = Vector3.Lerp(Vector3.up, Vector3.forward, progress);

            foreach (var d in Directions)
            {
                from = to;

                to = Vector3.Lerp(Vector3.up,  d, progress);

                t = GenerateUpperStrip(i, v, vBottom, t, triangles);

                v = GenerateVertexLine(from, to, i, v, vertices);

                vBottom += i + 1;
            }

            vBottom = v - 1 - i * 4;
        }

        // Close the loop at the top of the shape
        for (var i = 0; i < 4; i++) {
            triangles[t++] = vBottom;
            triangles[t++] = v;
            triangles[t++] = ++vBottom;
            vertices[v++] = Vector3.up;
        }
    }

    private static void Normalize(in Vector3[] vertices, out Vector3[] normals)
    {
        normals = new Vector3[vertices.Length];
        for (var i = 0; i < vertices.Length; i++)
        {
            normals[i] = vertices[i] = vertices[i].normalized;
        }
    }

    private static void CreateUV(in Vector3[] vertices, out Vector2[] uv)
    {
        uv = new Vector2[vertices.Length];

        var prevX = 1f;

        for (var i = 0; i < vertices.Length; i++)
        {
            var v = vertices[i];

            if (Math.Abs(v.x - prevX) < float.Epsilon)
            {
                uv[i - 1].x = 1f;
            }

            prevX = v.x;

            var texCoords = new Vector2
            {
                x = Mathf.Atan2(v.x, v.z) / (-2f * Mathf.PI),
                y = Mathf.Asin(v.y) / Mathf.PI + 0.5f
            };
            if (texCoords.x < 0f)
            {
                texCoords.x += 1f;
            }

            uv[i] = texCoords;
        }

        uv[vertices.Length - 4].x = uv[0].x = 0.125f;
        uv[vertices.Length - 3].x = uv[1].x = 0.375f;
        uv[vertices.Length - 2].x = uv[2].x = 0.625f;
        uv[vertices.Length - 1].x = uv[3].x = 0.875f;
    }
}
