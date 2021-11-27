using Scratch.Settings;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using float2x4 = Unity.Mathematics.float2x4;
using float4 = Unity.Mathematics.float4;

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

            public void Execute(int i)
            {
                Vertices[i] = normalize(Vertices[i]);
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
        private struct HeightJob : IJobFor
        {

            public NativeArray<float3> Vertices;
            public float Radius;
            public float Seed;
            public float Scale;

            public void Execute(int i)
            {
                var noiseSum = 0f;
                var amplitude = Scale;
                var frequency = 1f;

                var v = Vertices[i];
                for (var k = 0; k < 5; k++)
                {
                    // Sample noise function and add to the result
                    noiseSum += noise.snoise(float4(v * frequency, Seed)) * amplitude;
                    // Make each layer more and more detailed
                    frequency *= 2;
                    // Make each layer contribute less and less to result
                    amplitude *= 0.5f;
                }
                Vertices[i] *= Radius - 0.5f + noiseSum;
            }
        }

        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
        private struct RockyPlanetHeightJob : IJobFor
        {
            public NativeArray<float3> Vertices;
            public float Radius;

            // Continent settings
            public float OceanFloorDepth;
            public float OceanDepthMultiplier;
            public float OceanFloorSmoothing;
            public float MountainBlend;

            public float4x3 NoiseParamsContinents;
            public float4x3 NoiseParamsMask;
            public float4x3 NoiseParamsMountains;

            public void Execute(int i)
            {
                var v = Vertices[i];

                var continentShape = FractalNoise(v, NoiseParamsContinents);

                continentShape = SmoothMax(continentShape, -OceanFloorDepth, OceanFloorSmoothing);

                if (continentShape < 0)
                {
                    continentShape *= 1 + OceanDepthMultiplier;
                }

                var ridgeNoise = SmoothedRidgidNoise(v, NoiseParamsMountains);

                var mask = Blend(0, MountainBlend, FractalNoise(v, NoiseParamsMask));
                // Calculate final height
                var finalHeight = 1 + continentShape * 0.01f + ridgeNoise * 0.01f * mask;

                Vertices[i] = v * (Radius + finalHeight);
            }

            private static float FractalNoise(float3 pos, float4x3 param)
            {
                // Extract parameters for readability
                var offset = param[0].xyz;
                var numLayers = (int)param[0].w;
                var persistence = param[1].x;
                var lacunarity = param[1].y;
                var scale = param[1].z;
                var multiplier = param[1].w;
                var verticalShift = param[2].x;

                // Sum up noise layers
                float noiseSum = 0;
                float amplitude = 1;
                var frequency = scale;
                for (var i = 0; i < numLayers; i ++) {
                    noiseSum += noise.snoise(pos * frequency + offset) * amplitude;
                    amplitude *= persistence;
                    frequency *= lacunarity;
                }
                return noiseSum * multiplier + verticalShift;
            }

            private static float RidgidNoise(float3 pos, float4x3 param) {
                // Extract parameters for readability
                var offset = param[0].xyz;
                var numLayers = (int)param[0].w;
                var persistence = param[1].x;
                var lacunarity = param[1].y;
                var scale = param[1].z;
                var multiplier = param[1].w;
                var power = param[2].x;
                var gain = param[2].y;
                var verticalShift = param[2].z;

                // Sum up noise layers
                float noiseSum = 0;
                float amplitude = 1;
                var frequency = scale;
                float ridgeWeight = 1;

                for (var i = 0; i < numLayers; i ++) {
                    var noiseVal = 1 - abs(noise.snoise(pos * frequency + offset));
                    noiseVal = pow(abs(noiseVal), power);
                    noiseVal *= ridgeWeight;
                    ridgeWeight = saturate(noiseVal * gain);

                    noiseSum += noiseVal * amplitude;
                    amplitude *= persistence;
                    frequency *= lacunarity;
                }
                return noiseSum * multiplier + verticalShift;
            }

            // Sample the noise several times at small offsets from the centre and average the result
            // This reduces some of the harsh jaggedness that can occur
            private static float SmoothedRidgidNoise(float3 pos, float4x3 param) {
                var sphereNormal = normalize(pos);
                var axisA = cross(sphereNormal, float3(0f,1f,0f));
                var axisB = cross(sphereNormal, axisA);

                var offsetDst = param[2].w * 0.01f;
                var sample0 = RidgidNoise(pos, param);
                var sample1 = RidgidNoise(pos - axisA * offsetDst, param);
                var sample2 = RidgidNoise(pos + axisA * offsetDst, param);
                var sample3 = RidgidNoise(pos - axisB * offsetDst, param);
                var sample4 = RidgidNoise(pos + axisB * offsetDst, param);
                return (sample0 + sample1 + sample2 + sample3 + sample4) / 5;
            }

            private static float Blend(float startHeight, float blendDst, float height)
            {
                return smoothstep(startHeight - blendDst / 2, startHeight + blendDst / 2, height);
            }

            // Smooth maximum of two values, controlled by smoothing factor k
            // When k = 0, this behaves identically to max(a, b)
            private static float SmoothMax(float a, float b, float k) {
                k = min(0, -k);
                var h = max(0, min(1, (b - a + k) / (2 * k)));
                return a * h + b * (1 - h) - k * h * (1 - h);
            }
        }

        public static Mesh Create(int subdivisions, float radius, FractalNoiseSettings continents, RidgeNoiseSettings mountains, FractalNoiseSettings mask)
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
            using var uv = new NativeArray<float2>(numVerts, Allocator.TempJob);

            var handle = new VertAndTriCreationJob
            {
                Vertices = vertices,
                Triangles = triangles,
                Resolution = resolution,
            }.Schedule();


            handle = new NormalizationJob
            {
                Vertices = vertices,
            }.ScheduleParallel(numVerts, resolution, handle);


            handle = new UVCreationJob
            {
                Vertices = vertices,
                UV = uv,
            }.Schedule(handle);

            // handle = new HeightJob
            // {
            //     Vertices = vertices,
            //     Radius = radius,
            //     Seed = seed,
            //     Scale = noiseScale,
            // }.ScheduleParallel(numVerts, resolution, handle);

            handle = new RockyPlanetHeightJob
            {
                Vertices = vertices,
                Radius = radius,
                OceanFloorDepth = 1.5f,
                OceanDepthMultiplier = 5,
                OceanFloorSmoothing = 0.5f,
                MountainBlend = 1.2f,
                NoiseParamsContinents = (float4x3) continents,
                NoiseParamsMask = (float4x3) mask,
                NoiseParamsMountains = (float4x3) mountains,
            }.ScheduleParallel(numVerts, resolution, handle);

            handle.Complete();

            var mesh = new Mesh
            {
                name = "Planet",
                vertices = vertices.Reinterpret<Vector3>().ToArray(),
                uv = uv.Reinterpret<Vector2>().ToArray(),
                triangles = triangles.ToArray(),
                normals = new Vector3[numVerts],
                tangents = new Vector4[numVerts],
            };
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            return mesh;
        }
    }
}
