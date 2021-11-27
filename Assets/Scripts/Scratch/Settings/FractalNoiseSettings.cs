using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace Scratch.Settings
{

    [System.Serializable]
    public class FractalNoiseSettings
    {
        public int numLayers = 4;
        public float lacunarity = 2;
        public float persistence = 0.5f;
        public float scale = 1;
        public float elevation = 1;
        public float verticalShift = 0;
        public Vector3 offset;

        public static explicit operator float4x3(FractalNoiseSettings s) =>
            float4x3(
                float4(s.offset, s.numLayers),
                float4(s.persistence, s.lacunarity, s.scale, s.elevation),
                float4(s.verticalShift, 0f, 0f, 0f)
            );
    }
}
