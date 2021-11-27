using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace Scratch.Settings
{
    [System.Serializable]
    public class RidgeNoiseSettings {
        public int numLayers = 5;
        public float lacunarity = 2;
        public float persistence = 0.5f;
        public float scale = 1;
        public float power = 2;
        public float elevation = 1;
        public float gain = 1;
        public float verticalShift = 0;
        public float peakSmoothing = 0;
        public Vector3 offset;

        public static explicit operator float4x3(RidgeNoiseSettings s) =>
            float4x3(
                float4(s.offset, s.numLayers),
                float4(s.persistence, s.lacunarity, s.scale, s.elevation),
                float4(s.power, s.gain, s.verticalShift, s.peakSmoothing)
            );
    }
}
