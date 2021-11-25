using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace Scratch
{
    public static class PlanetCreator
    {

        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
        private struct VertAndTriCreationJob : IJob
        {
            private static readonly float3x4 Directions = new float3x4(
                float3(-1f, 0f, 0f), // Left
                float3(0f, 0f, -1f), // Back
                float3(1f, 0f, 0f), // Right
                float3(0f, 0f, 1f) // Forward
            );

            public NativeArray<float3> Vertices;
            public NativeArray<int> Triangles;
            public int Resolution;

            public void Execute()
            {
                int v = 0, vBottom = 0, t = 0;

                for (var i = 0; i < 4; i++)
                {
                    Vertices[v++] = float3(0f, -1f, 0f); // Down
                }

                for (var i = 1; i <= Resolution; i++)
                {
                    var progress = (float) i / Resolution;
                    float3 from, to;
                    // lerp(Down, Forward, progress)
                    Vertices[v++] = to = lerp(float3(0f, -1f, 0f), float3(0f, 0f, 1f), progress);
                    for (var d = 0; d < 4; d++)
                    {
                        from = to;
                        // lerp(Down, Directions[d], progress)
                        to = lerp(float3(0f, -1f, 0f), Directions[d], progress);
                        t = CreateLowerStrip(i, v, vBottom, t);
                        v = CreateVertexLine(from, to, i, v);
                        vBottom += i > 1 ? (i - 1) : 1;
                    }
                    vBottom = v - 1 - i * 4;
                }

                for (var i = Resolution - 1; i >= 1; i--)
                {
                    var progress = (float) i / Resolution;
                    float3 from, to;
                    // lerp(Up, Forward, progress)
                    Vertices[v++] = to = lerp(float3(0f, 1f, 0f), float3(0f, 0f, 1f), progress);
                    for (var d = 0; d < 4; d++)
                    {
                        from = to;
                        // lerp(Up, Directions[d], progress)
                        to = lerp(float3(0f, 1f, 0f), Directions[d], progress);
                        t = CreateUpperStrip(i, v, vBottom, t);
                        v = CreateVertexLine(from, to, i, v);
                        vBottom += i + 1;
                    }
                    vBottom = v - 1 - i * 4;
                }

                for (var i = 0; i < 4; i++)
                {
                    Triangles[t++] = vBottom;
                    Triangles[t++] = v;
                    Triangles[t++] = ++vBottom;
                    Vertices[v++] = float3(0f, 1f, 0f); // Up
                }
            }

            private int CreateVertexLine (float3 from, float3 to, int steps, int v) {
                for (var i = 1; i <= steps; i++) {
                    Vertices[v++] = lerp(from, to, (float)i / steps);
                }
                return v;
            }

            private int CreateLowerStrip(int steps, int vTop, int vBottom, int t)
            {
                for (var i = 1; i < steps; i++) {
                    Triangles[t++] = vBottom;
                    Triangles[t++] = vTop - 1;
                    Triangles[t++] = vTop;

                    Triangles[t++] = vBottom++;
                    Triangles[t++] = vTop++;
                    Triangles[t++] = vBottom;
                }
                Triangles[t++] = vBottom;
                Triangles[t++] = vTop - 1;
                Triangles[t++] = vTop;
                return t;
            }

            private int CreateUpperStrip(int steps, int vTop, int vBottom, int t)
            {
                Triangles[t++] = vBottom;
                Triangles[t++] = vTop - 1;
                Triangles[t++] = ++vBottom;
                for (var i = 1; i <= steps; i++) {
                    Triangles[t++] = vTop - 1;
                    Triangles[t++] = vTop;
                    Triangles[t++] = vBottom;

                    Triangles[t++] = vBottom;
                    Triangles[t++] = vTop++;
                    Triangles[t++] = ++vBottom;
                }
                return t;
            }
        }

        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
        private struct NormalizationJob : IJobFor
        {
            public NativeArray<float3> Vertices;

            [WriteOnly]
            public NativeArray<float3> Normals;

            public void Execute(int i)
            {
                Normals[i] = Vertices[i] = normalize(Vertices[i]);
            }
        }

        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
        private struct UVCreationJob : IJob
        {
            public NativeArray<float3> Vertices;
            public NativeArray<float2> UV;

            public void Execute()
            {
                var previousX = 1f;
                var len = Vertices.Length;
                float2 texCoords;
                for (var i = 0; i < len; i++)
                {
                    var v = Vertices[i];
                    if (abs(v.x - previousX) < float.Epsilon)
                    {
                        texCoords = UV[i - 1];
                        texCoords.x = 1f;
                        UV[i - 1] = texCoords;
                    }
                    previousX = v.x;
                    texCoords = float2(atan2(v.x, v.z) / (-2f * PI), asin(v.y) / PI + 0.5f);
                    if (texCoords.x < 0f)
                    {
                        texCoords.x += 1f;
                    }
                    UV[i] = texCoords;
                }

                texCoords = UV[len - 4];
                texCoords.x = 0.125f;
                UV[len - 4] = texCoords;

                texCoords = UV[len - 3];
                texCoords.x = 0.375f;
                UV[len - 4] = texCoords;

                texCoords = UV[len - 2];
                texCoords.x = 0.625f;
                UV[len - 4] = texCoords;

                texCoords = UV[len - 1];
                texCoords.x = 0.875f;
                UV[len - 4] = texCoords;
            }
        }

        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
        private struct TangentCreationJob : IJob
        {
            public NativeArray<float3> Vertices;

            [WriteOnly]
            public NativeArray<float4> Tangents;

            public void Execute()
            {
                var len = Vertices.Length;
                for (var i = 0; i < len; i++)
                {
                    var v = Vertices[i];
                    v.y = 0f;
                    v = normalize(v);
                    Tangents[i] = new float4(-v.z, 0f, v.x, -1f);
                }

                Tangents[len - 4] = Tangents[0] = float4(normalize(float3(-1f, 0, -1f)), -1f);
                Tangents[len - 3] = Tangents[1] = float4(normalize(float3(1f, 0f, -1f)), -1f);
                Tangents[len - 2] = Tangents[2] = float4(normalize(float3(1f, 0f, 1f)), -1f);
                Tangents[len - 1] = Tangents[3] = float4(normalize(float3(-1f, 0f, 1f)), -1f);
            }
        }

        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
        private struct ScaleJob : IJobFor
        {
            public NativeArray<float3> Vertices;
            public float Radius;

            public void Execute(int i)
            {
                Vertices[i] *= Radius;
            }
        }

        public static Mesh Create(int subdivisions, float radius)
        {
            if (subdivisions < 0) {
                subdivisions = 0;
                Debug.LogWarning("Planet subdivisions increased to minimum, which is 0.");
            }
            else if (subdivisions > 6) {
                subdivisions = 6;
                Debug.LogWarning("Planet subdivisions decreased to maximum, which is 6.");
            }

            var resolution = 1 << subdivisions;
            var numVerts = (resolution + 1) * (resolution + 1) * 4 - (resolution * 2 - 1) * 3;
            using var vertices = new NativeArray<float3>(numVerts, Allocator.TempJob);
            using var triangles = new NativeArray<int>((1 << (subdivisions * 2 + 3)) * 3, Allocator.TempJob);
            using var normals = new NativeArray<float3>(numVerts, Allocator.TempJob);
            using var uv = new NativeArray<float2>(numVerts, Allocator.TempJob);
            using var tangents = new NativeArray<float4>(numVerts, Allocator.TempJob);

            var handle = new VertAndTriCreationJob
            {
                Vertices = vertices,
                Triangles = triangles,
                Resolution = resolution,

            }.Schedule();


            handle = new NormalizationJob
            {
                Vertices = vertices,
                Normals = normals,
            }.ScheduleParallel(numVerts, resolution, handle);

            handle = new UVCreationJob
            {
                Vertices = vertices,
                UV = uv,
            }.Schedule(handle);

            handle = new TangentCreationJob
            {
                Vertices = vertices,
                Tangents = tangents,
            }.Schedule(handle);

            if (Mathf.Abs(radius - 1f) > float.Epsilon)
            {
                handle = new ScaleJob
                {
                    Vertices = vertices,
                    Radius = radius,
                }.ScheduleParallel(numVerts, resolution, handle);
            }

            handle.Complete();

            return new Mesh
            {
                name = "Planet",
                vertices = vertices.Reinterpret<Vector3>().ToArray(),
                normals = normals.Reinterpret<Vector3>().ToArray(),
                uv = uv.Reinterpret<Vector2>().ToArray(),
                tangents = tangents.Reinterpret<Vector4>().ToArray(),
                triangles = triangles.ToArray(),
            };
        }
    }
}
