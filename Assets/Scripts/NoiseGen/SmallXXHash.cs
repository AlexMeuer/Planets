using Unity.Mathematics;

namespace NoiseGen
{
    // ReSharper disable once InconsistentNaming
    public readonly struct SmallXXHash
    {
        private const uint primeA = 0b10011110001101110111100110110001;
        private const uint primeB = 0b10000101111010111100101001110111;
        private const uint primeC = 0b11000010101100101010111000111101;
        private const uint primeD = 0b00100111110101001110101100101111;
        private const uint primeE = 0b00010110010101100110011110110001;

        public static SmallXXHash Seed(int seed) => (uint) seed + primeE;

        private readonly uint _accumulator;

        public SmallXXHash(uint accumulator)
        {
            _accumulator = accumulator;
        }

        public SmallXXHash Eat(int data) => RotateLeft(_accumulator + (uint)data * primeC, 17) * primeD;

        public SmallXXHash Eat (byte data) => RotateLeft(_accumulator + data * primeE, 11) * primeA;

        private static uint RotateLeft (uint data, int steps) => (data << steps) | (data >> 32 - steps);

        public static implicit operator SmallXXHash (uint accumulator) => new SmallXXHash(accumulator);

        public static implicit operator uint(SmallXXHash hash)
        {
            var avalanche = hash._accumulator;
            avalanche ^= avalanche >> 15;
            avalanche *= primeB;
            avalanche ^= avalanche >> 13;
            avalanche *= primeC;
            avalanche ^= avalanche >> 16;
            return avalanche;
        }

        public static implicit operator SmallXXHash4(SmallXXHash hash) => new SmallXXHash4(hash._accumulator);
    }

    // ReSharper disable once InconsistentNaming
    public readonly struct SmallXXHash4
    {
        private const uint primeB = 0b10000101111010111100101001110111;
        private const uint primeC = 0b11000010101100101010111000111101;
        private const uint primeD = 0b00100111110101001110101100101111;
        private const uint primeE = 0b00010110010101100110011110110001;

        public static SmallXXHash4 Seed(int seed) => (uint4) seed + primeE;

        private readonly uint4 _accumulator;

        public SmallXXHash4(uint4 accumulator)
        {
            _accumulator = accumulator;
        }

        public SmallXXHash4 Eat(int4 data) => RotateLeft(_accumulator + (uint4)data * primeC, 17) * primeD;

        private static uint4 RotateLeft (uint4 data, int steps) => (data << steps) | (data >> 32 - steps);

        public static implicit operator SmallXXHash4 (uint4 accumulator) => new SmallXXHash4(accumulator);

        public static implicit operator uint4(SmallXXHash4 hash)
        {
            var avalanche = hash._accumulator;
            avalanche ^= avalanche >> 15;
            avalanche *= primeB;
            avalanche ^= avalanche >> 13;
            avalanche *= primeC;
            avalanche ^= avalanche >> 16;
            return avalanche;
        }
    }
}
