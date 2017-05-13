using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSDS.Utilities
{
    public interface Randomness
    {
        /// <summary>
        /// Gets 64 random bits as a long, probably to be consumed by RNG.
        /// </summary>
        /// <returns>a pseudo-random long that can be positive or negative</returns>
        long Next64();
        /// <summary>
        /// Gets 32 random bits as an int, probably to be consumed by RNG.
        /// </summary>
        /// <returns>a pseudo-random int that can be positive or negative</returns>
        int Next32();
        /// <summary>
        /// Gets a byte array that can be saved or used to restore the state of this Randomness (using FromSnapshot() ).
        /// </summary>
        /// <returns>a byte array that contains enough data to restore the state of this Randomness</returns>
        byte[] GetSnapshot();
        /// <summary>
        /// Sets this Randomness' state so it matches the data stored in snapshot, which should have been
        /// obtained from an object of the same class as this one using GetSnapshot() .
        /// </summary>
        /// <param name="snapshot">a byte array produced by GetSnapshot() on an object with the same class as this</param>
        void FromSnapshot(byte[] snapshot);
    }

    public class RNG : Random
    {
        public static Random GlobalRandom = new Random();
        public Randomness rand;
        /// <summary>
        /// Constructs an RNG with a SplitMixRandomness, randomly seeded, as its Randomness.
        /// </summary>
        public RNG()
        {
            rand = new SplitMixRandomness();
        }

        public RNG(long seed)
        {
            rand = new SplitMixRandomness(seed);
        }

        /// <summary>
        /// Constructs an RNG with randomSource used as its Randomness.
        /// </summary>
        public RNG(Randomness randomSource)
        {
            rand = randomSource;
        }
        /// <summary>
        /// Returns a pseudo-random long, which can be positive or negative and have any 64-bit value.
        /// </summary>
        /// <returns>any int, all 64 bits are pseudo-random</returns>

        public long NextLong()
        {
            return rand.Next64();
        }
        /// <summary>
        /// Gets a random int that is between 0 (inclusive) and maxValue (exclusive), which must be
        /// positive (if it is 0 or less, this simply returns 0).
        /// </summary>
        /// <param name="maxValue">the exclusive upper bound, which should be 1 or greater</param>
        /// <returns>a pseudo-random long between 0 (inclusive) and maxValue (exclusive)</returns>

        public long NextLong(long maxValue)
        {
            if(maxValue <= 0) return 0;
            long threshold = (0x7fffffffffffffffL - maxValue + 1) % maxValue;
            for(;;)
            {
                long bits = rand.Next64() & 0x7fffffffffffffffL;
                if(bits >= threshold)
                    return bits % maxValue;
            }
        }
        /// <summary>
        /// Gets a random long that is between minValue (inclusive) and maxValue (exclusive);
        /// both should be positive and minValue should be less than maxValue.
        /// </summary>
        /// <param name="minValue">the lower bound as a long, inclusive</param>
        /// <param name="maxValue">the upper bound as a long, exclusive</param>
        /// <returns></returns>
        public long NextLong(long minValue, long maxValue)
        {
            return NextLong(maxValue - minValue) + minValue;
        }

        /// <summary>
        /// Returns a pseudo-random int, which can be positive or negative and have any 32-bit value.
        /// </summary>
        /// <returns>any int, all 32 bits are pseudo-random</returns>
        public int NextInt()
        {
            return rand.Next32();
        }
        /// <summary>
        /// Returns a positive pseudo-random int, which can have any 31-bit positive value.
        /// </summary>
        /// <returns>any random positive int, all but the sign bit are pseudo-random</returns>
        public override int Next()
        {
            return rand.Next32() & 0x7fffffff;
        }
        /// <summary>
        /// Gets a random int that is between 0 (inclusive) and maxValue (exclusive), which can be positive or negative.
        /// </summary>
        /// <remarks>Based on code by Daniel Lemire, http://lemire.me/blog/2016/06/27/a-fast-alternative-to-the-modulo-reduction/ </remarks>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public override int Next(int maxValue)
        {
            return (int)((maxValue * (rand.Next64() & 0x7FFFFFFFL)) >> 31);
        }
        /// <summary>
        /// Gets a random int that is between minValue (inclusive) and maxValue (exclusive); both can be positive or negative.
        /// </summary>
        /// <param name="minValue">the inner bound as an int, inclusive</param>
        /// <param name="maxValue">the outer bound as an int, exclusive</param>
        /// <returns></returns>
        public override int Next(int minValue, int maxValue)
        {
            return Next(maxValue - minValue) + minValue;
        }
        /// <summary>
        /// Fills buffer with random values, from its start to its end.
        /// </summary>
        /// <remarks>
        /// Based on reference code in the documentation for java.util.Random, but modified
        /// to work with 8 random bytes at a time instead of 4.
        /// </remarks>
        /// <param name="buffer">a non-null byte array that will be modified</param>
        public override void NextBytes(byte[] buffer)
        {
            if(buffer == null)
                throw new ArgumentNullException("buffer");
            for(int i = 0; i < buffer.Length;)
                for(long r = rand.Next64(), n = Math.Min(buffer.Length - i, 8); n-- > 0; r >>= 8)
                    buffer[i++] = (byte)r;
        }
        /// <summary>
        /// Gets a random double between 0.0 (inclusive) and 1.0 (exclusive).
        /// </summary>
        /// <remarks>
        /// This uses a technique by Sebastiano Vigna, described at http://xoroshiro.di.unimi.it/#remarks
        /// </remarks>
        /// <returns>a pseudo-random double between 0.0 inclusive and 1.0 exclusive</returns>
        public override double NextDouble()
        {
            return BitConverter.Int64BitsToDouble(0x3FF0000000000000L | (rand.Next64() & 0x000FFFFFFFFFFFFFL)) - 1.0;
        }
        protected override double Sample()
        {
            return BitConverter.Int64BitsToDouble(0x3FF0000000000000L | (rand.Next64() & 0x000FFFFFFFFFFFFFL)) - 1.0;
        }
    }

    public class SplitMixRandomness : Randomness
    {
        public ulong State;

        public SplitMixRandomness()
        {
            State = (ulong)RNG.GlobalRandom.Next() ^ (ulong)RNG.GlobalRandom.Next() << 21 ^ (ulong)RNG.GlobalRandom.Next() << 42;
        }

        public SplitMixRandomness(ulong state)
        {
            State = state;
        }

        public SplitMixRandomness(long state)
        {
            State = (ulong)state;
        }

        public void FromSnapshot(byte[] snapshot)
        {
            if(snapshot == null)
                throw new ArgumentNullException("snapshot");
            if(snapshot.Length < 8)
                State = (ulong)(-1L - snapshot.LongLength);
            State = BitConverter.ToUInt64(snapshot, 0);
        }

        public byte[] GetSnapshot()
        {
            return BitConverter.GetBytes(State);
        }

        public int Next32()
        {
            ulong z = (State += 0x9E3779B97F4A7C15UL);
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return (int)(z ^ (z >> 31));
        }

        public long Next64()
        {
            ulong z = (State += 0x9E3779B97F4A7C15UL);
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return (long)(z ^ (z >> 31));

        }
    }
}
