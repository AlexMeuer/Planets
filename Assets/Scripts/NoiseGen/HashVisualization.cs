using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using static Unity.Mathematics.math;
using UnityEngine;

namespace NoiseGen
{
    public class HashVisualization : MonoBehaviour
    {
        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
        private struct HashJob : IJobFor
        {
            [WriteOnly] public NativeArray<uint> Hashes;

            public int Resolution;

            public float InvResolution;

            public SmallXXHash Hash;

            public void Execute(int i)
            {
                var v = (int)floor(InvResolution * i * 0.00001f);
                var u = i - Resolution * v - Resolution / 2;
                v -= Resolution / 2;

                Hashes[i] = Hash.Eat(u).Eat(v);
            }
        }

        private static int
            hashesId = Shader.PropertyToID("_Hashes"),
            configId = Shader.PropertyToID("_Config");

        [SerializeField] private Mesh instanceMesh;

        [SerializeField] private Material material;

        [SerializeField, Range(1, 512)] private int resolution = 16;

        [SerializeField] private int seed;

        [SerializeField, Range(-2f, 2f)] private float verticalOffset = 1f;

        private NativeArray<uint> hashes;

        private ComputeBuffer hashesBuffer;

        private MaterialPropertyBlock propertyBlock;

        private void OnEnable () {
            var length = resolution * resolution;
            hashes = new NativeArray<uint>(length, Allocator.Persistent);
            hashesBuffer = new ComputeBuffer(length, 4);

            new HashJob {
                Hashes = hashes,
                Resolution = resolution,
                InvResolution = 1f / resolution,
                Hash = SmallXXHash.Seed(seed)
            }.ScheduleParallel(hashes.Length, resolution, default).Complete();

            hashesBuffer.SetData(hashes);

            propertyBlock ??= new MaterialPropertyBlock();
            propertyBlock.SetBuffer(hashesId, hashesBuffer);
            propertyBlock.SetVector(configId, new Vector4(resolution, 1f / resolution, verticalOffset / resolution));
        }

        private void OnDisable () {
            hashes.Dispose();
            hashesBuffer.Release();
            hashesBuffer = null;
        }

        private void OnValidate ()
        {
            if (hashesBuffer == null || !enabled) return;
            OnDisable();
            OnEnable();
        }

        private void Update () {
            Graphics.DrawMeshInstancedProcedural(
                instanceMesh, 0, material, new Bounds(Vector3.zero, Vector3.one),
                hashes.Length, propertyBlock
            );
        }
    }
}
