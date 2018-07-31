using System;

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
        /// <summary>
        /// Returns a copy of this Randomness type with a copied state.
        /// </summary>
        /// <returns>a copy of this Randomness type with a copied state</returns>
        Randomness Copy();
    }

    public class RNG : Random
    {
        public static Random GlobalRandom = new Random();
        public Randomness Rand { get; set; }
        /// <summary>
        /// Constructs an RNG with a SplitMixRandomness, randomly seeded, as its Randomness.
        /// </summary>
        public RNG()
        {
            Rand = new SplitMixRandomness();
        }

        public RNG(long seed)
        {
            Rand = new SplitMixRandomness(seed);
        }

        /// <summary>
        /// Constructs an RNG with randomSource used as its Randomness.
        /// </summary>
        public RNG(Randomness randomSource)
        {
            Rand = randomSource;
        }
        /// <summary>
        /// Returns a pseudo-random long, which can be positive or negative and have any 64-bit value.
        /// </summary>
        /// <returns>any int, all 64 bits are pseudo-random</returns>

        public long NextLong()
        {
            return Rand.Next64();
        }
        /// <summary>
        /// Gets a random int that is between 0 (inclusive) and maxValue (exclusive), which must be
        /// positive (if it is 1 or less, this simply returns 0).
        /// </summary>
        /// <remarks>Credit to user craigster0 from https://stackoverflow.com/a/28904636 for the code this uses, which gets the high 64 bits of a 64-by-64-bit multiplication.</remarks>
        /// <param name="maxValue">the exclusive upper bound, which should be 1 or greater</param>
        /// <returns>a pseudo-random long between 0 (inclusive) and maxValue (exclusive)</returns>

        public long NextLong(long maxValue)
        {
            if (maxValue <= 1L)
                return 0L;
            unchecked
            {
                ulong a = (ulong)Rand.Next64();
                ulong a_lo = a & 0xFFFFFFFFUL;
                ulong a_hi = a >> 32;
                ulong b_lo = (ulong)maxValue & 0xFFFFFFFFUL;
                ulong b_hi = (ulong)maxValue >> 32;

                ulong a_x_b_hi = a_hi * b_hi;
                ulong a_x_b_mid = a_hi * b_lo;
                ulong b_x_a_mid = b_hi * a_lo;
                ulong a_x_b_lo = a_lo * b_lo;

                ulong carry_bit = ((a_x_b_mid & 0xFFFFFFFFUL) +
                                   (b_x_a_mid & 0xFFFFFFFFUL) +
                                   (a_x_b_lo >> 32)) >> 32;

                return (long)(a_x_b_hi + (a_x_b_mid >> 32) + (b_x_a_mid >> 32) + carry_bit);
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
            return Rand.Next32();
        }
        /// <summary>
        /// Returns a positive pseudo-random int, which can have any 31-bit positive value.
        /// </summary>
        /// <returns>any random positive int, all but the sign bit are pseudo-random</returns>
        public override int Next()
        {
            return Rand.Next32() & 0x7fffffff;
        }
        /// <summary>
        /// Gets a random int that is between 0 (inclusive) and maxValue (exclusive), which can be positive or negative.
        /// </summary>
        /// <remarks>Based on code by Daniel Lemire, http://lemire.me/blog/2016/06/27/a-fast-alternative-to-the-modulo-reduction/ </remarks>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public override int Next(int maxValue)
        {
            return (int)((maxValue * (Rand.Next64() & 0x7FFFFFFFL)) >> 31);
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
                for(long r = Rand.Next64(), n = Math.Min(buffer.Length - i, 8); n-- > 0; r >>= 8)
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
            return BitConverter.Int64BitsToDouble(0x3FF0000000000000L | (Rand.Next64() & 0x000FFFFFFFFFFFFFL)) - 1.0;
        }
        /// <summary>
        /// Gets a random double between 0.0 (inclusive) and 1.0 (exclusive).
        /// </summary>
        /// <remarks>
        /// The same code as NextDouble().
        /// This uses a technique by Sebastiano Vigna, described at http://xoroshiro.di.unimi.it/#remarks
        /// </remarks>
        /// <returns>a pseudo-random double between 0.0 inclusive and 1.0 exclusive</returns>
        protected override double Sample()
        {
            return BitConverter.Int64BitsToDouble(0x3FF0000000000000L | (Rand.Next64() & 0x000FFFFFFFFFFFFFL)) - 1.0;
        }
        /// <summary>
        /// Returns a new RNG using the same algorithm and a copy of the internal state this uses.
        /// Calling the same methods on this RNG and its copy should produce the same values.
        /// </summary>
        /// <returns>a copy of this RNG</returns>
        public RNG Copy()
        {
            return new RNG(Rand.Copy());
        }
        /// <summary>
        /// Gets a snapshot of the current state as a byte array. This snapshot can be used to restore the current state.
        /// </summary>
        /// <remarks>
        /// Normally, you get a byte array by calling this method on this RNG, and later call FromSnapshot() on this RNG and
        /// give it the earlier byte array. This can be useful for saving state, but it only works if the Randomness implementations
        /// are the same. The default Randomness is SplitMixRandomness, so if you didn't specify a different one, then the snapshots
        /// from and to those default RNGs will be compatible.
        /// </remarks>
        /// <returns>a snapshot of the current state as a byte array</returns>
        public byte[] GetSnapshot()
        {
            return Rand.GetSnapshot();
        }
        /// <summary>
        /// Restores the state this uses internally to the one stored in snapshot, a byte array.
        /// </summary>
        /// <param name="snapshot">a byte array normally produced by GetSnapshot() called on this RNG or its Randomness</param>
        public void FromSnapshot(byte[] snapshot)
        {
            Rand.FromSnapshot(snapshot);
        }
    }

    public class SplitMixRandomness : Randomness
    {
        public ulong State;

        public SplitMixRandomness()
        {
            State = (ulong)RNG.GlobalRandom.Next() >> 5 ^ (ulong)RNG.GlobalRandom.Next() << 21 ^ (ulong)RNG.GlobalRandom.Next() << 42;
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
                State = (ulong)(-1L - snapshot.LongLength * 421L);
            else
                State = BitConverter.ToUInt64(snapshot, 0);
        }

        public byte[] GetSnapshot()
        {
            return BitConverter.GetBytes(State);
        }

        public Randomness Copy()
        {
            return new SplitMixRandomness(State);
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
    /// <summary>
    /// SMA is short for Split, Mix, Alter, which refers to the SplitMix32 step this performs with a specific increment
    /// until a certain point is reached, when it then alters the increment and keeps going.
    /// </summary>
    public class SMARandomness : Randomness
    {
        public uint State, Inc;

        public SMARandomness()
            : this((uint)RNG.GlobalRandom.Next(), (uint)RNG.GlobalRandom.Next())
        {
        }

        public SMARandomness(uint state)
        {
            State = determine(state + 19) + state;
            Inc = determine(State + state) | 1U;
        }

        public SMARandomness(uint state, uint inc)
        {
            State = state;
            Inc = inc | 1U;
        }
        public SMARandomness(ulong state)
            : this((uint)(state), (uint)(state >> 32))
        {
        }

        public void FromSnapshot(byte[] snapshot)
        {
            if(snapshot == null)
                throw new ArgumentNullException("snapshot");
            if(snapshot.Length < 8)
            {
                State = (uint)(181U + snapshot.Length * 421U);
                Inc = determine(State + 181U) | 1U;
            }
            else
            {
                State = BitConverter.ToUInt32(snapshot, 0);
                Inc = BitConverter.ToUInt32(snapshot, 4) | 1U;
            }
        }

        public byte[] GetSnapshot()
        {
            return
                (BitConverter.IsLittleEndian)
                ? BitConverter.GetBytes(State | ((ulong)Inc << 32))
                : BitConverter.GetBytes(Inc | ((ulong)State << 32));
        }

        public Randomness Copy()
        {
            return new SMARandomness(State, Inc);
        }

        public int Next32()
        {
            uint z = (State += (State == 0U) ? (Inc += 0x632BE5A6U) : Inc); //0x9E3779B9U);//
            z = (z ^ (z >> 16)) * 0x85EBCA6BU;
            z = (z ^ (z >> 13)) * 0xC2B2AE35U;
            return (int)(z ^ (z >> 16));
        }

        public long Next64()
        {
            uint y = (State += (State == 0) ? (Inc += 0x632BE5A6) : Inc), //0x9E3779B9U);//
                    z = (State += (State == 0) ? (Inc += 0x632BE5A6) : Inc);//0x9E3779B9U);//
            y = (y ^ (y >> 16)) * 0x85EBCA6B;
            y = (y ^ (y >> 13)) * 0xC2B2AE35;
            z = (z ^ (z >> 16)) * 0x85EBCA6B;
            z = (z ^ (z >> 13)) * 0xC2B2AE35;
            return (long)(y ^ (y >> 16)) << 32 ^ (z ^ (z >> 16));
        }
        public static uint determine(uint state)
        {
            state = ((state *= 0x9E3779B9U) ^ (state >> 16)) * 0x85EBCA6BU;
            state = (state ^ (state >> 13)) * 0xC2B2AE35U;
            return state ^ (state >> 16);
        }

    }

    /// <summary>
    /// Based on Thrust32RNG in the Sarong project (which is in Java, and has the same author, me).
    /// </summary>
    public class ThrustRandomness : Randomness
    {
        public uint State;

        public ThrustRandomness()
            : this((uint)RNG.GlobalRandom.Next())
        {
        }

        public ThrustRandomness(uint state)
        {
            State = state;
        }

        public void FromSnapshot(byte[] snapshot)
        {
            if(snapshot == null)
                throw new ArgumentNullException("snapshot");
            if(snapshot.Length < 4)
            {
                State = (uint)(181U + snapshot.Length * 421U);
            }
            else
            {
                State = BitConverter.ToUInt32(snapshot, 0);
            }
        }

        public byte[] GetSnapshot()
        {
            return BitConverter.GetBytes(State);
        }

        public Randomness Copy()
        {
            return new ThrustRandomness(State);
        }

        public int Next32()
        {
            uint z = (State += 0x7F4A7C15U);
            z = (z ^ z >> 14) * (0x41C64E6DU + (z & 0x7FFEU));
            return (int)(z ^ z >> 13);
        }

        public long Next64()
        {
            uint x = State + 0x7F4A7C15U, y = (State += 0xFE94F82AU);
            x = (x ^ x >> 14) * (0x41C64E6DU + (x & 0x7FFEU));
            y = (y ^ y >> 14) * (0x41C64E6DU + (y & 0x7FFEU));
            return (long)(x ^ x >> 13) << 32 ^ (y ^ y >> 13);
        }
        public static uint determine(uint state)
        {
            state = ((state *= 0x7F4A7C15U) ^ state >> 14) * (0x41C64E6DU + (state & 0x7FFEU));
            return state ^ state >> 13;
        }
    }

    public class RushRandomness : Randomness
    {
        public long State0, State1;

        public RushRandomness() : this(RNG.GlobalRandom.Next() << 15 ^ RNG.GlobalRandom.Next(),
            RNG.GlobalRandom.Next() << 14 ^ RNG.GlobalRandom.Next(), RNG.GlobalRandom.Next() << 16 ^ RNG.GlobalRandom.Next())
        {
        }
        public RushRandomness(long seed)
        {
            State0 = seed * -0x3943D8696D4A3B7DL - 0x7CD6391461952C1DL;
            State1 = seed * -0x7CD6391461952C1DL + 0x3943D8696D4A3B7DL;
        }

        public RushRandomness(long state0, long state1)
        {
            State0 = state0;
            State1 = state1;
        }

        public RushRandomness(int seed0, int seed1, int seed2)
        {
            State0 = (seed0 * 0xBFL + seed1 * seed2 << 24) ^ -0x7CD6391461952C1DL;
            State1 = (seed1 * -0x7CD6391461952C1DL ^ seed2 - -0x3943D8696D4A3B7DL) - seed0 * 0x61C8864680B583EBL;
        }


        public void FromSnapshot(byte[] snapshot)
        {
            if(snapshot == null)
                throw new ArgumentNullException("snapshot");
            if(snapshot.Length < 16)
            {
                State0 = (-0x3943D8696D4A3B7DL - snapshot.LongLength * 0x7CD6391461952C1DL);
                State1 = (0x7CD6391461952C1DL + snapshot.LongLength * 0x3943D8696D4A3B7DL);
            }
            else
            {
                State0 = BitConverter.ToInt64(snapshot, 0);
                State1 = BitConverter.ToInt64(snapshot, 8);
            }
        }

        public byte[] GetSnapshot()
        {
            byte[] snap = new byte[16];
            Buffer.BlockCopy(new long[] { State0, State1 }, 0, snap, 0, 16);
            return snap;
        }

        public Randomness Copy()
        {
            return new RushRandomness(State0, State1);
        }

        public int Next32()
        {
            return (int)(State1 += ((State0 -= 0x61C8864680B583EBL) >> 24) * 0x632AE59B69B3C209L);
        }

        public long Next64()
        {
            return State1 += ((State0 -= 0x61C8864680B583EBL) >> 24) * 0x632AE59B69B3C209L;
        }
    }
    public class HerdRandomness : Randomness
    {
        public uint choice = 0U;
        public uint[] state = new uint[16];

        public HerdRandomness()
        {
            for(int i = 0; i < 16; i++)
            {
                choice += (state[i] = (uint)(RNG.GlobalRandom.Next() << (9 + i) ^ RNG.GlobalRandom.Next()));
            }
        }
        public HerdRandomness(int seed)
        {
            uint seed2 = (uint)seed, p;
            seed2 = ((seed2 >> 19 | seed2 << 13) ^ 0x13A5BA1DU);
            for(int i = 0; i < 16; i++)
            {
                p = (seed2 += 0x9E3779B9U);
                p ^= p >> (4 + (int)(p >> 28));
                choice += (state[i] = ((p *= 277803737) >> 22) ^ p);
            }
        }

        public HerdRandomness(int[] seed)
        {
            uint sum = 0U, temp, p;
            if(seed == null)
            {
                temp = 0x13A5BA1DU;
                for(int i = 0; i< 16; i++)
                {
                    p = (temp += 0x9E3779B9U);
                    p ^= p >> (4 + (int)(p >> 28));
                    choice += (state[i] = ((p *= 277803737) >> 22) ^ p);
                }
}
            else
            {
                temp = 0U;
                for(int s = 0; s < seed.Length; s++)
                {
                    sum += (uint)seed[s];
                    temp += ((sum >> 19 | sum << 13) ^ 0x13A5BA1DU);
                    for(int i = 0; i< 16; i++)
                    {
                        p = (temp += 0x9E3779B9U);
                        p ^= p >> (4 + (int)(p >> 28));
                        choice += (state[i] ^= ((p *= 277803737) >> 22) ^ p);
                    }
                }
            }
        }
        public HerdRandomness(uint[] stateSeed, uint choiceSeed)
        {
            if(stateSeed == null || stateSeed.Length != 16)
            {
                for(int i = 0; i < 16; i++)
                {
                    choice += (state[i] = (uint)(RNG.GlobalRandom.Next() << (9 + i) ^ RNG.GlobalRandom.Next()));
                }
            }
            else
            {
                Buffer.BlockCopy(stateSeed, 0, state, 0, 64);
                choice = choiceSeed;
            }
        }

        public void FromSnapshot(byte[] snapshot)
        {
            if(snapshot == null)
                throw new ArgumentNullException("snapshot");
            if(snapshot.Length < 68)
            {
                uint seed2 = (uint)snapshot.Length, p;
                seed2 = ((seed2 >> 19 | seed2 << 13) ^ 0x13A5BA1DU);
                for(int i = 0; i < 16; i++)
                {
                    p = (seed2 += 0x9E3779B9U);
                    p ^= p >> (4 + (int)(p >> 28));
                    state[i] = ((p *= 277803737) >> 22) ^ p;
                }
                p = (seed2 += 0x8D265FCDU);
                p ^= p >> (4 + (int)(p >> 28));
                choice = ((p *= 277803737) >> 22) ^ p;

            }
            else
            {
                Buffer.BlockCopy(snapshot, 64, state, 0, 4);
                choice = state[0];
                Buffer.BlockCopy(snapshot, 0, state, 0, 64);
            }
        }

        public byte[] GetSnapshot()
        {
            byte[] snap = new byte[68];
            Buffer.BlockCopy(state, 0, snap, 0, 64);
            Buffer.BlockCopy(new uint[] { choice }, 0, snap, 64, 4);
            return snap;
        }

        public Randomness Copy()
        {
            return new HerdRandomness(state, choice);
        }

        public int Next32()
        {
            return (int)(state[(choice += 0x9CBC276DU) & 15] += (state[choice >> 28] + 0xBA3779D9U >> 1));
        }

        public long Next64()
        {
            return (state[(choice += 0x9CBC276DU) & 15] += (state[choice >> 28] + 0xBA3779D9U >> 1))
                * 0x632AE59B69B3C209L - choice;
        }
    }
    /// <summary>
    /// Very close to RNG with HerdRandomness hard-coded as its Randomness, but a fair amount faster thanks to less overhead.
    /// </summary>
    /// <remarks>
    /// PRNG4 (in this same namespace) is recommended as an update over this class, since it has higher quality.
    /// Uses a different "format" of snapshot that this can process more easily, a uint array instead of a byte array.
    /// </remarks>
    public class PRNG : Random
    {
        //public static Random GlobalRandom = new Random();

        public uint choice = 0U;
        public uint[] state = new uint[16];

        /// <summary>
        /// Constructs a randomly-seeded PRNG using 32 calls of System.Random's Next() method on a global Random.
        /// </summary>
        public PRNG()
        {
            for(int i = 0; i < 16; i++)
            {
                choice += (state[i] = (uint)(RNG.GlobalRandom.Next() << (9 + i) ^ RNG.GlobalRandom.Next()));
            }
        }
        /// <summary>
        /// Constructs a PRNG using just one int for a seed. A PRNG object has 17 ints of total state, so the set of
        /// distinct PRNGs that can be produced by this constuctor is much smaller than the total set of PRNGs possible.
        /// Consider using the constructor that takes an int array, or a uint array and a uint.
        /// </summary>
        /// <param name="seed">any int</param>
        public PRNG(int seed)
        {
            uint seed2 = (uint)seed, p;
            seed2 = ((seed2 >> 19 | seed2 << 13) ^ 0x13A5BA1DU);
            for(int i = 0; i < 16; i++)
            {
                p = (seed2 += 0x9E3779B9U);
                p ^= p >> (4 + (int)(p >> 28));
                choice += (state[i] = ((p *= 277803737) >> 22) ^ p);
            }
        }

        /// <summary>
        /// Constructs a PRNG while initializing the state (which is a uint array) using the given int array.
        /// Each item in the given int array will be used to iteratively modify each of the 16 state items, with
        /// the choice field starting at 0 but having each step of change to a state item added to it as a sum.
        /// You can give this an array with more or less than 16 elements, or even null, and it will still work.
        /// The order that values appear in the seed affects the resulting PRNG.
        /// </summary>
        /// <param name="seed">an int array, which can be null, empty, or any length</param>
        public PRNG(int[] seed)
        {
            uint sum = 0U, temp, p;
            if(seed == null)
            {
                temp = 0x13A5BA1DU;
                for(int i = 0; i < 16; i++)
                {
                    p = (temp += 0x9E3779B9U);
                    p ^= p >> (4 + (int)(p >> 28));
                    choice += (state[i] = ((p *= 277803737) >> 22) ^ p);
                }
            }
            else
            {
                temp = 0U;
                for(int s = 0; s < seed.Length; s++)
                {
                    sum += (uint)seed[s];
                    temp += ((sum >> 19 | sum << 13) ^ 0x13A5BA1DU);
                    for(int i = 0; i < 16; i++)
                    {
                        p = (temp += 0x9E3779B9U);
                        p ^= p >> (4 + (int)(p >> 28));
                        choice += (state[i] ^= ((p *= 277803737) >> 22) ^ p);
                    }
                }
            }
        }
        /// <summary>
        /// Attempts to reproduce the exact state and choice values given as parameters.
        /// </summary>
        /// <remarks>
        /// If stateSeed is null or is less than 16 elements in length, this will fall back to
        /// the behavior of the zero-argument constructor.
        /// </remarks>
        /// <param name="stateSeed">an array of uint that should be 16 elements long (it can be longer, but extra values won't be used)</param>
        /// <param name="choiceSeed">a uint that will be used in a slightly different way from the rest of the state, and so is separate</param>
        public PRNG(uint[] stateSeed, uint choiceSeed)
        {
            if(stateSeed == null || stateSeed.Length != 16)
            {
                for(int i = 0; i < 16; i++)
                {
                    choice += (state[i] = (uint)(RNG.GlobalRandom.Next() << (9 + i) ^ RNG.GlobalRandom.Next()));
                }
            }
            else
            {
                Buffer.BlockCopy(stateSeed, 0, state, 0, 64);
                choice = choiceSeed;
            }
        }


        /// <summary>
        /// Returns a pseudo-random long, which can be positive or negative and have any 64-bit value.
        /// </summary>
        /// <returns>any int, all 64 bits are pseudo-random</returns>

        public long NextLong()
        {
            return ((state[(choice += 0x9CBC276DU) & 15] += (state[choice >> 28] + 0xBA3779D9U >> 1)) * 0x632AE59B69B3C209L - choice);
        }
        /// <summary>
        /// Gets a random int that is between 0 (inclusive) and maxValue (exclusive), which must be
        /// positive (if it is 1 or less, this simply returns 0).
        /// </summary>
        /// <remarks>Credit to user craigster0 from https://stackoverflow.com/a/28904636 for the code this uses, which gets the high 64 bits of a 64-by-64-bit multiplication.</remarks>
        /// <param name="maxValue">the exclusive upper bound, which should be 1 or greater</param>
        /// <returns>a pseudo-random long between 0 (inclusive) and maxValue (exclusive)</returns>

        public long NextLong(long maxValue)
        {
            if (maxValue <= 1L)
                    return 0L;
            unchecked {
                ulong a = (ulong)NextLong();
                ulong a_lo = a & 0xFFFFFFFFUL;
                ulong a_hi = a >> 32;
                ulong b_lo = (ulong)maxValue & 0xFFFFFFFFUL;
                ulong b_hi = (ulong)maxValue >> 32;

                ulong a_x_b_hi = a_hi * b_hi;
                ulong a_x_b_mid = a_hi * b_lo;
                ulong b_x_a_mid = b_hi * a_lo;
                ulong a_x_b_lo = a_lo * b_lo;

                ulong carry_bit = ((a_x_b_mid & 0xFFFFFFFFUL) +
                                   (b_x_a_mid & 0xFFFFFFFFUL) +
                                   (a_x_b_lo >> 32)) >> 32;

                return (long)(a_x_b_hi + (a_x_b_mid >> 32) + (b_x_a_mid >> 32) + carry_bit);
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
            return (int)(state[(choice += 0x9CBC276DU) & 15] += (state[choice >> 28] + 0xBA3779D9U >> 1));
        }
        /// <summary>
        /// Returns a positive pseudo-random int, which can have any 31-bit positive value.
        /// </summary>
        /// <returns>any random positive int, all but the sign bit are pseudo-random</returns>
        public override int Next()
        {
            return (int)(state[(choice += 0x9CBC276DU) & 15] += (state[choice >> 28] + 0xBA3779D9U >> 1)) & 0x7fffffff;
        }
        /// <summary>
        /// Gets a random int that is between 0 (inclusive) and maxValue (exclusive), which can be positive or negative.
        /// </summary>
        /// <remarks>Based on code by Daniel Lemire, http://lemire.me/blog/2016/06/27/a-fast-alternative-to-the-modulo-reduction/ </remarks>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public override int Next(int maxValue)
        {
            return (int)((maxValue * ((state[(choice += 0x9CBC276DU) & 15] += (state[choice >> 28] + 0xBA3779D9U >> 1)) & 0x7FFFFFFFL)) >> 31);
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
        /// Based on reference code in the documentation for java.util.Random.
        /// </remarks>
        /// <param name="buffer">a non-null byte array that will be modified</param>
        public override void NextBytes(byte[] buffer)
        {
            if(buffer == null)
                throw new ArgumentNullException("buffer");
            for(int i = 0; i < buffer.Length;)
            {
                uint r = (state[(choice += 0x9CBC276DU) & 15] += (state[choice >> 28] + 0xBA3779D9U >> 1));
                for(int n = Math.Min(buffer.Length - i, 4); n-- > 0; r >>= 4)
                    buffer[i++] = (byte)r;
            }
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
            return BitConverter.Int64BitsToDouble(0x3FF0000000000000L |
                (((state[(choice += 0x9CBC276DU) & 15] += (state[choice >> 28] + 0xBA3779D9U >> 1)) * 0x632AE59B69B3C209L - choice) & 0x000FFFFFFFFFFFFFL)) - 1.0;
        }
        /// <summary>
        /// Gets a random double between -1.0 (inclusive) and 1.0 (exclusive).
        /// </summary>
        /// <remarks>
        /// This uses a technique by Sebastiano Vigna, described at http://xoroshiro.di.unimi.it/#remarks
        /// </remarks>
        /// <returns>a pseudo-random double between -1.0 inclusive and 1.0 exclusive</returns>
        public double NextSignedDouble()
        {
            return BitConverter.Int64BitsToDouble(0x4000000000000000L |
                (((state[(choice += 0x9CBC276DU) & 15] += (state[choice >> 28] + 0xBA3779D9U >> 1)) * 0x632AE59B69B3C209L - choice) & 0x000FFFFFFFFFFFFFL)) - 3.0;
        }
        /// <summary>
        /// Gets a random double between 0.0 (inclusive) and 1.0 (exclusive).
        /// </summary>
        /// <remarks>
        /// The same code as NextDouble().
        /// This uses a technique by Sebastiano Vigna, described at http://xoroshiro.di.unimi.it/#remarks
        /// </remarks>
        /// <returns>a pseudo-random double between 0.0 inclusive and 1.0 exclusive</returns>
        protected override double Sample()
        {
            return BitConverter.Int64BitsToDouble(0x3FF0000000000000L |
                (((state[(choice += 0x9CBC276DU) & 15] += (state[choice >> 28] + 0xBA3779D9U >> 1)) * 0x632AE59B69B3C209L - choice) & 0x000FFFFFFFFFFFFFL)) - 1.0;
        }
        /// <summary>
        /// Returns a new RNG using the same algorithm and a copy of the internal state this uses.
        /// Calling the same methods on this RNG and its copy should produce the same values.
        /// </summary>
        /// <returns>a copy of this RNG</returns>
        public PRNG Copy()
        {
            return new PRNG(state, choice);
        }
        /// <summary>
        /// Gets a snapshot of the current state as a uint array. This snapshot can be used to restore the current state.
        /// </summary>
        /// <returns>a snapshot of the current state as a uint array</returns>
        public uint[] GetSnapshot()
        {
            uint[] snap = new uint[17];
            Array.Copy(state, snap, 16);
            snap[16] = choice;
            return snap;
        }

        /// <summary>
        /// Restores the state this uses internally to the one stored in snapshot, a uint array.
        /// </summary>
        /// <param name="snapshot">a uint array normally produced by GetSnapshot() called on this PRNG</param>
        public void FromSnapshot(uint[] snapshot)
        {
            if(snapshot == null)
                throw new ArgumentNullException("snapshot");
            if(snapshot.Length < 17)
            {
                uint seed2 = (uint)snapshot.Length, p;
                seed2 = ((seed2 >> 19 | seed2 << 13) ^ 0x13A5BA1DU);
                for(int i = 0; i < 16; i++)
                {
                    p = (seed2 += 0x9E3779B9U);
                    p ^= p >> (4 + (int)(p >> 28));
                    state[i] = ((p *= 277803737) >> 22) ^ p;
                }
                p = (seed2 += 0x8D265FCDU);
                p ^= p >> (4 + (int)(p >> 28));
                choice = ((p *= 277803737) >> 22) ^ p;

            }
            else
            {
                Array.Copy(snapshot, state, 16);
                choice = snapshot[16];
            }
        }

    }
    /// <summary>
    /// Very close to RNG with SMARandomness hard-coded as its Randomness, but a fair amount faster thanks to less overhead.
    /// </summary>
    /// <remarks>
    /// A good replacement for System.Random due to drastically higher speed, as well as comparable or better quality,
    /// a loadable and settable state via snapshots, and various other useful features, like NextInt() for 32-bit random values.
    /// Be advised that the period is small at 2 to the 32 (4294967296), and the sequence of generated numbers will repeat after
    /// 2 to the 32 calls to Next() or similar 32-bit methods (methods that produce long, ulong, or double values count as two calls).
    /// </remarks>
    public class SplitMixPRNG : Random
    {
        //public static Random GlobalRandom = new Random();

        public uint State;

        public SplitMixPRNG()
            : this((uint)RNG.GlobalRandom.Next())
        {
        }

        public SplitMixPRNG(uint state)
        {
            State = state;
        }

        public void FromSnapshot(byte[] snapshot)
        {
            if(snapshot == null)
                throw new ArgumentNullException("snapshot");
            if(snapshot.Length < 8)
            {
                State = (uint)(181U + snapshot.Length * 421U);
            }
            else
            {
                State = BitConverter.ToUInt32(snapshot, 0);
            }
        }

        public byte[] GetSnapshot()
        {
            return BitConverter.GetBytes(State);
        }

        public int Next32()
        {
            uint z = (State += 0x9E3779B9U);//(State == 0U) ? (Inc += 0x632BE5A6U) : Inc);
            z = (z ^ (z >> 16)) * 0x85EBCA6BU;
            z = (z ^ (z >> 13)) * 0xC2B2AE35U;
            return (int)(z ^ (z >> 16));
        }
        /// <summary>
        /// Returns a pseudo-random long, which can be positive or negative and have any 64-bit value.
        /// </summary>
        /// <returns>any int, all 64 bits are pseudo-random</returns>
        public long NextLong()
        {
            uint y = (State += 0x9E3779B9U),//(State == 0) ? (Inc += 0x632BE5A6) : Inc),
                    z = (State += 0x9E3779B9U);// (State == 0) ? (Inc += 0x632BE5A6) : Inc);
            y = (y ^ (y >> 16)) * 0x85EBCA6B;
            z = (z ^ (z >> 16)) * 0x85EBCA6B;
            y = (y ^ (y >> 13)) * 0xC2B2AE35;
            z = (z ^ (z >> 13)) * 0xC2B2AE35;
            return (long)(y ^ (y >> 16)) << 32 ^ (z ^ (z >> 16));
        }
        public static uint Determine(uint state)
        {
            state = ((state *= 0x9E3779B9U) ^ (state >> 16)) * 0x85EBCA6BU;
            state = (state ^ (state >> 13)) * 0xC2B2AE35U;
            return state ^ (state >> 16);
        }


        /// <summary>
        /// Gets a random int that is between 0 (inclusive) and maxValue (exclusive), which must be
        /// positive (if it is 1 or less, this simply returns 0).
        /// </summary>
        /// <remarks>Credit to user craigster0 from https://stackoverflow.com/a/28904636 for the code this uses, which gets the high 64 bits of a 64-by-64-bit multiplication.</remarks>
        /// <param name="maxValue">the exclusive upper bound, which should be 1 or greater</param>
        /// <returns>a pseudo-random long between 0 (inclusive) and maxValue (exclusive)</returns>

        public long NextLong(long maxValue)
        {
            if (maxValue <= 1L)
                return 0L;
            unchecked
            {
                ulong a = (ulong)NextLong();
                ulong a_lo = a & 0xFFFFFFFFUL;
                ulong a_hi = a >> 32;
                ulong b_lo = (ulong)maxValue & 0xFFFFFFFFUL;
                ulong b_hi = (ulong)maxValue >> 32;

                ulong a_x_b_hi = a_hi * b_hi;
                ulong a_x_b_mid = a_hi * b_lo;
                ulong b_x_a_mid = b_hi * a_lo;
                ulong a_x_b_lo = a_lo * b_lo;

                ulong carry_bit = ((a_x_b_mid & 0xFFFFFFFFUL) +
                                   (b_x_a_mid & 0xFFFFFFFFUL) +
                                   (a_x_b_lo >> 32)) >> 32;

                return (long)(a_x_b_hi + (a_x_b_mid >> 32) + (b_x_a_mid >> 32) + carry_bit);
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
            uint z = (State += 0x9E3779B9U);//(State == 0U) ? (Inc += 0x632BE5A6U) : Inc);
            z = (z ^ (z >> 16)) * 0x85EBCA6BU;
            z = (z ^ (z >> 13)) * 0xC2B2AE35U;
            return (int)(z ^ (z >> 16));
        }
        /// <summary>
        /// Returns a positive pseudo-random int, which can have any 31-bit positive value.
        /// </summary>
        /// <returns>any random positive int, all but the sign bit are pseudo-random</returns>
        public override int Next()
        {
            uint z = (State += 0x9E3779B9U);//(State == 0U) ? (Inc += 0x632BE5A6U) : Inc);
            z = (z ^ (z >> 16)) * 0x85EBCA6BU;
            z = (z ^ (z >> 13)) * 0xC2B2AE35U;
            return (int)(z ^ (z >> 16)) & 0x7fffffff;
        }
        /// <summary>
        /// Gets a random int that is between 0 (inclusive) and maxValue (exclusive), which can be positive or negative.
        /// </summary>
        /// <remarks>Based on code by Daniel Lemire, http://lemire.me/blog/2016/06/27/a-fast-alternative-to-the-modulo-reduction/ </remarks>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public override int Next(int maxValue)
        {
            uint z = (State += 0x9E3779B9U);//(State == 0U) ? (Inc += 0x632BE5A6U) : Inc);
            z = (z ^ (z >> 16)) * 0x85EBCA6BU;
            z = (z ^ (z >> 13)) * 0xC2B2AE35U;
            return (int)((maxValue * ((z ^ (z >> 16)) & 0x7FFFFFFFL)) >> 31);
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
        /// Based on reference code in the documentation for java.util.Random.
        /// </remarks>
        /// <param name="buffer">a non-null byte array that will be modified</param>
        public override void NextBytes(byte[] buffer)
        {
            if(buffer == null)
                throw new ArgumentNullException("buffer");
            for(int i = 0; i < buffer.Length;)
            {
                uint z = (State += 0x9E3779B9U);//(State == 0U) ? (Inc += 0x632BE5A6U) : Inc);
                z = (z ^ (z >> 16)) * 0x85EBCA6BU;
                z = (z ^ (z >> 13)) * 0xC2B2AE35U;
                z ^= (z >> 16);
                for(int n = Math.Min(buffer.Length - i, 4); n-- > 0; z >>= 4)
                    buffer[i++] = (byte)z;
            }
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
            uint y = (State += 0x9E3779B9U),//(State == 0) ? (Inc += 0x632BE5A6) : Inc),
                z = (State += 0x9E3779B9U);// (State == 0) ? (Inc += 0x632BE5A6) : Inc);
            y = (y ^ (y >> 16)) * 0x85EBCA6B;
            z = (z ^ (z >> 16)) * 0x85EBCA6B;
            y = (y ^ (y >> 13)) * 0xC2B2AE35;
            z = (z ^ (z >> 13)) * 0xC2B2AE35;

            return BitConverter.Int64BitsToDouble(0x3FF0000000000000L |
                (((long)(y ^ (y >> 16)) << 32 ^ (z ^ (z >> 16))) & 0x000FFFFFFFFFFFFFL)) - 1.0;
        }
        /// <summary>
        /// Gets a random double between -1.0 (inclusive) and 1.0 (exclusive).
        /// </summary>
        /// <remarks>
        /// This uses a technique by Sebastiano Vigna, described at http://xoroshiro.di.unimi.it/#remarks
        /// </remarks>
        /// <returns>a pseudo-random double between -1.0 inclusive and 1.0 exclusive</returns>
        public double NextSignedDouble()
        {
            uint y = (State += 0x9E3779B9U),//(State == 0) ? (Inc += 0x632BE5A6) : Inc),
                z = (State += 0x9E3779B9U);// (State == 0) ? (Inc += 0x632BE5A6) : Inc);
            y = (y ^ (y >> 16)) * 0x85EBCA6B;
            z = (z ^ (z >> 16)) * 0x85EBCA6B;
            y = (y ^ (y >> 13)) * 0xC2B2AE35;
            z = (z ^ (z >> 13)) * 0xC2B2AE35;

            return BitConverter.Int64BitsToDouble(0x4000000000000000L |
                (((long)(y ^ (y >> 16)) << 32 ^ (z ^ (z >> 16))) & 0x000FFFFFFFFFFFFFL)) - 3.0;
        }
        /// <summary>
        /// Gets a random double between 0.0 (inclusive) and 1.0 (exclusive).
        /// </summary>
        /// <remarks>
        /// The same code as NextDouble().
        /// This uses a technique by Sebastiano Vigna, described at http://xoroshiro.di.unimi.it/#remarks
        /// </remarks>
        /// <returns>a pseudo-random double between 0.0 inclusive and 1.0 exclusive</returns>
        protected override double Sample()
        {
            uint y = (State += 0x9E3779B9U),//(State == 0) ? (Inc += 0x632BE5A6) : Inc),
                z = (State += 0x9E3779B9U);// (State == 0) ? (Inc += 0x632BE5A6) : Inc);
            y = (y ^ (y >> 16)) * 0x85EBCA6B;
            z = (z ^ (z >> 16)) * 0x85EBCA6B;
            y = (y ^ (y >> 13)) * 0xC2B2AE35;
            z = (z ^ (z >> 13)) * 0xC2B2AE35;

            return BitConverter.Int64BitsToDouble(0x3FF0000000000000L |
                (((long)(y ^ (y >> 16)) << 32 ^ (z ^ (z >> 16))) & 0x000FFFFFFFFFFFFFL)) - 1.0;
        }
        /// <summary>
        /// Returns a new PRNG2 using the same algorithm and a copy of the internal state this uses.
        /// Calling the same methods on this PRNG2 and its copy should produce the same values.
        /// </summary>
        /// <returns>a copy of this PRNG2</returns>
        public SplitMixPRNG Copy()
        {
            return new SplitMixPRNG(State);
        }
    }
    /// <summary>
    /// Very close to RNG with ThrustRandomness hard-coded as its Randomness, but a fair amount faster thanks to less overhead.
    /// </summary>
    /// <remarks>
    /// Usually a good replacement for System.Random due to drastically higher speed, as well as better quality according to PractRand tests,
    /// a loadable and settable state via snapshots, and various other useful features, like NextInt() for 32-bit random values. It has
    /// a low period, however, of 2 to the 32 instead of the probably-higher period of System.Random (although System.Random has more state,
    /// it is not implemented in a way that matches its algorithm's definition, and may not be full-period). The primary weakness of the Thrust
    /// family of generators is that they cannot produce all possible outputs over their full period (where an output is a uint and the period
    /// is 2 to the 32). This 32-bit generator is closer to ThrustAlt than the original (more seriously-flawed) Thrust algorithm; ThrustAlt can
    /// pass quite a lot more testing than Thrust at 64 bits of state (at least 32 TB in PractRand instead of 16GB in PractRand), but at 32 bits
    /// of state and 4 bytes of output at a time, only 16 GB can be produced before the period is exhausted.
    /// </remarks>
    public class ThrustPRNG : Random
    {
        public uint State;

        public ThrustPRNG()
            : this((uint)RNG.GlobalRandom.Next())
        {
        }

        public ThrustPRNG(uint state)
        {
            State = state;
        }

        public void FromSnapshot(byte[] snapshot)
        {
            if(snapshot == null)
                throw new ArgumentNullException("snapshot");
            if(snapshot.Length < 8)
            {
                State = (uint)(181U + snapshot.Length * 421U);
            }
            else
            {
                State = BitConverter.ToUInt32(snapshot, 0);
            }
        }

        public byte[] GetSnapshot()
        {
            return BitConverter.GetBytes(State);
        }
        /// <summary>
        /// Returns a pseudo-random int, which can be positive or negative and have any 32-bit value.
        /// </summary>
        /// <returns>any int, all 32 bits are pseudo-random</returns>
        public int NextInt()
        {
            uint z = (State += 0x7F4A7C15U);
            z = (z ^ z >> 14) * (0x41C64E6DU + (z & 0x7FFEU));
            return (int)(z ^ z >> 13);
        }
        /// <summary>
        /// Returns a pseudo-random long, which can be positive or negative and have any 64-bit value.
        /// </summary>
        /// <returns>any int, all 64 bits are pseudo-random</returns>
        public long NextLong()
        {
            uint x = State + 0x7F4A7C15U, y = (State += 0xFE94F82AU);
            x = (x ^ x >> 14) * (0x41C64E6DU + (x & 0x7FFEU));
            y = (y ^ y >> 14) * (0x41C64E6DU + (y & 0x7FFEU));
            return (long)(x ^ x >> 13) << 32 ^ (y ^ y >> 13);
        }

        public static uint determine(uint state)
        {
            state = ((state *= 0x7F4A7C15U) ^ state >> 14) * (0x41C64E6DU + (state & 0x7FFEU));
            return state ^ state >> 13;
        }

        /// <summary>
        /// Gets a random int that is between 0 (inclusive) and maxValue (exclusive), which must be
        /// positive (if it is 1 or less, this simply returns 0).
        /// </summary>
        /// <remarks>Credit to user craigster0 from https://stackoverflow.com/a/28904636 for the code this uses, which gets the high 64 bits of a 64-by-64-bit multiplication.</remarks>
        /// <param name="maxValue">the exclusive upper bound, which should be 1 or greater</param>
        /// <returns>a pseudo-random long between 0 (inclusive) and maxValue (exclusive)</returns>

        public long NextLong(long maxValue)
        {
            if (maxValue <= 1L)
                return 0L;
            unchecked
            {
                ulong a = (ulong)NextLong();
                ulong a_lo = a & 0xFFFFFFFFUL;
                ulong a_hi = a >> 32;
                ulong b_lo = (ulong)maxValue & 0xFFFFFFFFUL;
                ulong b_hi = (ulong)maxValue >> 32;

                ulong a_x_b_hi = a_hi * b_hi;
                ulong a_x_b_mid = a_hi * b_lo;
                ulong b_x_a_mid = b_hi * a_lo;
                ulong a_x_b_lo = a_lo * b_lo;

                ulong carry_bit = ((a_x_b_mid & 0xFFFFFFFFUL) +
                                   (b_x_a_mid & 0xFFFFFFFFUL) +
                                   (a_x_b_lo >> 32)) >> 32;

                return (long)(a_x_b_hi + (a_x_b_mid >> 32) + (b_x_a_mid >> 32) + carry_bit);
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
        /// Returns a positive pseudo-random int, which can have any 31-bit positive value.
        /// </summary>
        /// <returns>any random positive int, all but the sign bit are pseudo-random</returns>
        public override int Next()
        {
            uint z = (State += 0x7F4A7C15U);
            z = (z ^ z >> 14) * (0x41C64E6DU + (z & 0x7FFEU));
            return (int)(z ^ z >> 13) & 0x7fffffff;
        }
        /// <summary>
        /// Gets a random int that is between 0 (inclusive) and maxValue (exclusive), which can be positive or negative.
        /// </summary>
        /// <remarks>Based on code by Daniel Lemire, http://lemire.me/blog/2016/06/27/a-fast-alternative-to-the-modulo-reduction/ </remarks>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public override int Next(int maxValue)
        {
            uint z = (State += 0x7F4A7C15U);
            z = (z ^ z >> 14) * (0x41C64E6DU + (z & 0x7FFEU));
            return (int)((maxValue * ((z ^ (z >> 13)) & 0x7FFFFFFFL)) >> 31);
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
        /// Based on reference code in the documentation for java.util.Random.
        /// </remarks>
        /// <param name="buffer">a non-null byte array that will be modified</param>
        public override void NextBytes(byte[] buffer)
        {
            if(buffer == null)
                throw new ArgumentNullException("buffer");
            for(int i = 0; i < buffer.Length;)
            {
                uint z = (State += 0x7F4A7C15U);
                z = (z ^ z >> 14) * (0x41C64E6DU + (z & 0x7FFEU));
                z ^= (z >> 13);
                for(int n = Math.Min(buffer.Length - i, 4); n-- > 0; z >>= 4)
                    buffer[i++] = (byte)z;
            }
        }
        /// <summary>
        /// Gets a random double between 0.0 (inclusive) and 1.0 (exclusive).
        /// </summary
        /// <returns>a pseudo-random double between 0.0 inclusive and 1.0 exclusive</returns>
        public override double NextDouble()
        {
            uint x = State + 0x7F4A7C15U, y = (State += 0xFE94F82AU);
            x = (x ^ x >> 14) * (0x41C64E6DU + (x & 0x7FFEU));
            y = (y ^ y >> 14) * (0x41C64E6DU + (y & 0x7FFEU));
            return (((long)(x ^ x >> 13) << 32 ^ (y ^ y >> 13)) & 0x1FFFFFFFFFFFFFL) * 1.1102230246251565E-16;
        }
        /// <summary>
        /// Gets a random double between -1.0 (exclusive) and 1.0 (exclusive).
        /// </summary>
        /// <returns>a pseudo-random double between -1.0 exclusive and 1.0 exclusive</returns>
        public double NextSignedDouble()
        {
            uint x = State + 0x7F4A7C15U, y = (State += 0xFE94F82AU);
            x = (x ^ x >> 14) * (0x41C64E6DU + (x & 0x7FFEU));
            y = (y ^ y >> 14) * (0x41C64E6DU + (y & 0x7FFEU));
            return (((long)(x ^ x >> 13) << 32 ^ (y ^ y >> 13)) >> 11) * 1.1102230246251565E-16;
        }
        /// <summary>
        /// Gets a random double between 0.0 (inclusive) and 1.0 (exclusive).
        /// </summary>
        /// <remarks>
        /// The same code as NextDouble().
        /// </remarks>
        /// <returns>a pseudo-random double between 0.0 inclusive and 1.0 exclusive</returns>
        protected override double Sample()
        {
            uint x = State + 0x7F4A7C15U, y = (State += 0xFE94F82AU);
            x = (x ^ x >> 14) * (0x41C64E6DU + (x & 0x7FFEU));
            y = (y ^ y >> 14) * (0x41C64E6DU + (y & 0x7FFEU));
            return (((long)(x ^ x >> 13) << 32 ^ (y ^ y >> 13)) & 0x1FFFFFFFFFFFFFL) * 1.1102230246251565E-16;
        }
        /// <summary>
        /// Returns a new PRNG3 using the same algorithm and a copy of the internal state this uses.
        /// Calling the same methods on this PRNG3 and its copy should produce the same values.
        /// </summary>
        /// <returns>a copy of this PRNG3</returns>
        public ThrustPRNG Copy()
        {
            return new ThrustPRNG(State);
        }
    }
    /// <summary>
    /// Very close to RNG with a higher-quality variant of HerdRandomness hard-coded as its Randomness, but a fair amount faster
    /// than RNG thanks to less overhead.
    /// </summary>
    /// <remarks>
    /// Uses a different "format" of snapshot that this can process more easily, a uint array instead of a byte array.
    /// A good replacement for System.Random due to drastically higher speed and period, as well as comparable or better quality,
    /// a loadable and settable state via snapshots, and various other useful features, like NextInt() for 32-bit random values.
    /// This passes PractRand statistical quality tests on 64MB of random uints, with no anomalies or failures (PRNG has a few failures).
    /// </remarks>
    public class PRNG4 : Random
    {
        public static Random GlobalRandom = new Random();

        public uint choice = 0U;
        public uint[] state = new uint[16];

        public PRNG4()
        {
            for(int i = 0; i < 16; i++)
            {
                choice += (state[i] = (uint)(GlobalRandom.Next() << (9 + i) ^ GlobalRandom.Next()));
            }
        }
        public PRNG4(int seed)
        {
            uint seed2 = (uint)seed;
            for(uint i = 0; i < 16U; i++)
            {
                choice += (state[i] = Randomize(seed2 + i * 0x7F4A7C15U));
            }
        }

        public PRNG4(int[] seed)
        {
            if(seed == null || seed.Length <= 0) seed = new int[1];
            uint sum = 0;
            for(int s = 0; s < seed.Length; s++)
            {
                sum += (uint)seed[s];
                for(uint i = 0; i < 16; i++)
                {
                    choice += (state[i] ^= Randomize(sum + i * 0x7F4A7C15U));
                }
            }
        }
        public PRNG4(uint[] stateSeed, uint choiceSeed)
        {
            if(stateSeed == null || stateSeed.Length != 16)
            {
                for(int i = 0; i < 16; i++)
                {
                    choice += (state[i] = (uint)(GlobalRandom.Next() << (9 + i) ^ GlobalRandom.Next()));
                }
            }
            else
            {
                Buffer.BlockCopy(stateSeed, 0, state, 0, 64);
                choice = choiceSeed;
            }
        }


        /// <summary>
        /// Returns a pseudo-random long, which can be positive or negative and have any 64-bit value.
        /// </summary>
        /// <returns>any int, all 64 bits are pseudo-random</returns>

        public long NextLong()
        {
            return (state[(choice += 0x9CBC276DU) & 15] += (state[choice >> 28] >> 13) + 0x5F356495) * 0x2C9277B500000000L ^
            (state[(choice += 0x9CBC276DU) & 15] += (state[choice >> 28] >> 13) + 0x5F356495) * 0x2C9277B5;
        }
        /// <summary>
        /// Gets a pseudo-random int that is between 0 (inclusive) and maxValue (exclusive); maxValue must be
        /// positive (if it is 1 or less, this simply returns 0).
        /// </summary>
        /// <remarks>Credit to user craigster0 from https://stackoverflow.com/a/28904636 for the code this uses, which gets the high 64 bits of a 64-by-64-bit multiplication.</remarks>
        /// <param name="maxValue">the exclusive upper bound, which should be 1 or greater</param>
        /// <returns>a pseudo-random long between 0 (inclusive) and maxValue (exclusive)</returns>

        public long NextLong(long maxValue)
        {
            if (maxValue <= 1L)
                return 0L;
            unchecked
            {
                ulong a = (ulong)NextLong();
                ulong a_lo = a & 0xFFFFFFFFUL;
                ulong a_hi = a >> 32;
                ulong b_lo = (ulong)maxValue & 0xFFFFFFFFUL;
                ulong b_hi = (ulong)maxValue >> 32;

                ulong a_x_b_hi = a_hi * b_hi;
                ulong a_x_b_mid = a_hi * b_lo;
                ulong b_x_a_mid = b_hi * a_lo;
                ulong a_x_b_lo = a_lo * b_lo;

                ulong carry_bit = ((a_x_b_mid & 0xFFFFFFFFUL) +
                                   (b_x_a_mid & 0xFFFFFFFFUL) +
                                   (a_x_b_lo >> 32)) >> 32;

                return (long)(a_x_b_hi + (a_x_b_mid >> 32) + (b_x_a_mid >> 32) + carry_bit);
            }
        }
        /// <summary>
        /// Gets a pseudo-random long that is between minValue (inclusive) and maxValue (exclusive);
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
            return (int)((state[(choice += 0x9CBC276DU) & 15] += (state[choice >> 28] >> 13) + 0x5F356495) * 0x2C9277B5);
        }
        /// <summary>
        /// Returns a positive pseudo-random int, which can have any 31-bit positive value.
        /// </summary>
        /// <returns>any random positive int, all but the sign bit are pseudo-random</returns>
        public override int Next()
        {
            return (int)((state[(choice += 0x9CBC276DU) & 15] += (state[choice >> 28] >> 13) + 0x5F356495) * 0x2C9277B5);
        }
        /// <summary>
        /// Gets a pseudo-random int that is between 0 (inclusive) and maxValue (exclusive), which can be positive or negative.
        /// </summary>
        /// <remarks>Based on code by Daniel Lemire, http://lemire.me/blog/2016/06/27/a-fast-alternative-to-the-modulo-reduction/ </remarks>
        /// <param name="maxValue"></param>
        /// <returns>an int between 0 and maxValue</returns>
        public override int Next(int maxValue)
        {
            return (int)((maxValue * (((state[(choice += 0x9CBC276DU) & 15] += (state[choice >> 28] >> 13) + 0x5F356495) * 0x2C9277B5) & 0x7FFFFFFFL)) >> 31);
        }
        /// <summary>
        /// Gets a pseudo-random int that is between minValue (inclusive) and maxValue (exclusive); both can be positive or negative.
        /// </summary>
        /// <param name="minValue">the inner bound as an int, inclusive</param>
        /// <param name="maxValue">the outer bound as an int, exclusive</param>
        /// <returns>an int between minValue (inclusive) and maxValue (exclusive)</returns>
        public override int Next(int minValue, int maxValue)
        {
            return Next(maxValue - minValue) + minValue;
        }
        /// <summary>
        /// Fills buffer with random values, from its start to its end.
        /// </summary>
        /// <remarks>
        /// Based on reference code in the documentation for java.util.Random.
        /// </remarks>
        /// <param name="buffer">a non-null byte array that will be modified</param>
        public override void NextBytes(byte[] buffer)
        {
            if(buffer == null)
                throw new ArgumentNullException("buffer");
            for(int i = 0; i < buffer.Length;)
            {
                uint r = ((state[(choice += 0x9CBC276DU) & 15] += (state[choice >> 28] >> 13) + 0x5F356495) * 0x2C9277B5);
                for(int n = Math.Min(buffer.Length - i, 4); n-- > 0; r >>= 8)
                    buffer[i++] = (byte)r;
            }
        }
        /// <summary>
        /// Gets a random double between 0.0 (inclusive) and 1.0 (exclusive).
        /// </summary>
        /// <returns>a pseudo-random double between 0.0 inclusive and 1.0 exclusive</returns>
        public override double NextDouble()
        {
            return (((state[(choice += 0x9CBC276DU) & 15] += (state[choice >> 28] >> 13) + 0x5F356495) * 0x2C9277B500000000L ^
            (state[(choice += 0x9CBC276DU) & 15] += (state[choice >> 28] >> 13) + 0x5F356495) * 0x2C9277B5) & 0x1FFFFFFFFFFFFFL) * 1.1102230246251565E-16;
        }
        /// <summary>
        /// Gets a random double between 0.0 (inclusive) and 1.0 (exclusive).
        /// </summary>
        /// <remarks>
        /// The same code as NextDouble().
        /// </remarks>
        /// <returns>a pseudo-random double between 0.0 inclusive and 1.0 exclusive</returns>
        protected override double Sample()
        {
            return (((state[(choice += 0x9CBC276DU) & 15] += (state[choice >> 28] >> 13) + 0x5F356495) * 0x2C9277B500000000L ^
            (state[(choice += 0x9CBC276DU) & 15] += (state[choice >> 28] >> 13) + 0x5F356495) * 0x2C9277B5) & 0x1FFFFFFFFFFFFFL) * 1.1102230246251565E-16;
        }
        /// <summary>
        /// Returns a new RNG using the same algorithm and a copy of the internal state this uses.
        /// Calling the same methods on this RNG and its copy should produce the same values.
        /// </summary>
        /// <returns>a copy of this RNG</returns>
        public PRNG4 Copy()
        {
            return new PRNG4(state, choice);
        }
        /// <summary>
        /// Gets a snapshot of the current state as a uint array. This snapshot can be used to restore the current state.
        /// </summary>
        /// <returns>a snapshot of the current state as a uint array</returns>
        public uint[] GetSnapshot()
        {
            uint[] snap = new uint[17];
            Array.Copy(state, snap, 16);
            snap[16] = choice;
            return snap;
        }

        /// <summary>
        /// Restores the state this uses internally to the one stored in snapshot, a uint array.
        /// </summary>
        /// <param name="snapshot">a uint array normally produced by GetSnapshot() called on this PRNG4</param>
        public void FromSnapshot(uint[] snapshot)
        {
            if(snapshot == null)
                throw new ArgumentNullException("snapshot");
            if(snapshot.Length < 17)
            {
                uint seed2 = Randomize((uint)snapshot.Length * 0x8D265FCDU);
                for(uint i = 0; i < 16; i++)
                {
                    state[i] = Randomize(seed2 + i * 0x7F4A7C15U);
                }
                choice = Randomize(Randomize(seed2 - 0x7F4A7C15U));

            }
            else
            {
                Array.Copy(snapshot, state, 16);
                choice = snapshot[16];
            }
        }

        /// <summary>
        /// Returns a random permutation of state; if state is the same on two calls to this, this will return the same number.
        /// This is expected to be called with <code>Randomize(state += 0x7F4A7C15U)</code> to generate a sequence of random numbers, or with
        /// <code>Randomize(state -= 0x7F4A7C15U)</code> to go backwards in the same sequence. Using other constants for the increment does not
        /// guarantee quality in the same way; in particular, using <code>Randomize(++state)</code> yields poor results for quality, and other
        /// very small numbers will likely also be low-quality.
        /// </summary>
        /// <param name="state">A UInt32 that should be different every time you want a different random result; use <code>Randomize(state += 0x7F4A7C15U)</code> ideally.</param>
        /// <returns>A pseudo-random permutation of state.</returns>
        public static uint Randomize(uint state)
        {
            state = (state ^ state >> 14) * (0x41C64E6DU + (state & 0x7FFEU));
            return state ^ state >> 13;
        }

    }
    /// <summary>
    /// A 64-bit class that's close to RNG with ThrustAlt hard-coded as its Randomness; this is the fastest generator here for most x64 targets.
    /// ThrustAlt is based on ThrustAltRNG from the Sarong and SquidLib projects in Java. It has very high quality, passing PractRand on 32 TB of
    /// generated random data, TestU01's full BigCrush suite with no failures, and gjrand very often (gjrand gives both a final score and a number of
    /// failures of various degrees; Thrust's 64-bit variant never goes below 0.1 on final score and only rarely has failures above the first
    /// rank, which it considers probably insignificant. To contrast, System.Random fails in single-digit GB on PractRand and only accrues more failures
    /// as testing continues, across multiple test families. It also has a decent period of 2 to the 64 (which could be better than System.Random).
    /// While it is fastest on x64 targets, it isn't especially slow on x86 either. The crucial weakness of Thrust and ThrustAlt is that they can't 
    /// produce all possible outputs over their full period (2 to the 64 possible states correspond to less than 2 to the 64 outputs). This still doesn't
    /// seem to hamper it in testing, however, and it may be good enough for a lot of usage.
    /// </summary>
    /// <remarks>
    /// Usually a good replacement for System.Random due to drastically higher speed (and possibly period), as well as significantly better quality,
    /// a loadable and settable state via snapshots, and various other useful features, like NextInt() for 32-bit random values, and Skip(long) to jump
    /// ahead or behind in its sequence. It has a period of 2 to the 64.
    /// </remarks>
    public class TAPRNG : Random
    {
        public ulong State;

        public TAPRNG()
            : this((ulong)RNG.GlobalRandom.Next() >> 5 ^ (ulong)RNG.GlobalRandom.Next() << 21 ^ (ulong)RNG.GlobalRandom.Next() << 42)
        {
        }

        public TAPRNG(ulong state)
        {
            State = state;
        }

        public void FromSnapshot(byte[] snapshot)
        {
            if (snapshot == null)
                throw new ArgumentNullException("snapshot");
            if (snapshot.Length < 8)
                State = (ulong)(-1L - snapshot.LongLength * 421L);
            else
                State = BitConverter.ToUInt64(snapshot, 0);
        }

        public byte[] GetSnapshot()
        {
            return BitConverter.GetBytes(State);
        }
        /// <summary>
        /// Returns a pseudo-random int, which can be positive or negative and have any 32-bit value.
        /// </summary>
        /// <returns>any int, all 32 bits are pseudo-random</returns>
        public int NextInt()
        {
            ulong s = (State += 0x6C8E9CF570932BD5UL);
            s = (s ^ (s >> 25)) * (s | 0xA529UL);
            return (int)(s ^ (s >> 22));

        }
        /// <summary>
        /// Returns a pseudo-random long, which can be positive or negative and have any 64-bit value.
        /// </summary>
        /// <returns>any long, all 64 bits are pseudo-random</returns>
        public long NextLong()
        {
            ulong s = (State += 0x6C8E9CF570932BD5UL);
            s = (s ^ (s >> 25)) * (s | 0xA529UL);
            return (long)(s ^ (s >> 22));
        }
        /// <summary>
        /// Jumps ahead or behind in the sequence of random numbers this returns, then returns a pseudo-random long as with NextLong() at the given point in the sequence.
        /// Positive values for the distance parameter will skip ahead, as if multiple calls were made to NextLong(), while negative values will skip behind, returning values
        /// from earlier in the sequence. The value this returns can be positive or negative and have any 64-bit value.
        /// </summary>
        /// <param name="distance">If positive, how many steps to skip ahead before producing a value; if negative, how many to skip behind</param>
        /// <returns>any long, all 64 bits are pseudo-random</returns>
        public long Skip(long distance)
        {
            ulong s = (State += (ulong)(0x6C8E9CF570932BD5L * distance));
            s = (s ^ (s >> 25)) * (s | 0xA529UL);
            return (long)(s ^ (s >> 22));
        }
        /// <summary>
        /// Jumps ahead or behind in the sequence of random numbers this returns, returning nothing but changing the next value that will be returned.
        /// Positive values for the distance parameter will skip ahead, as if multiple calls were made to NextLong(), while negative values will skip behind, returning values
        /// from earlier in the sequence.
        /// </summary>
        /// <param name="distance">If positive, how many steps to skip ahead before producing a value; if negative, how many to skip behind</param>

        public void Advance(long distance)
        {
            State += (ulong)(0x6C8E9CF570932BD5L * distance);
        }
        /// <summary>
        /// Returns a pseudo-random unsigned long, which can have any 64-bit value.
        /// </summary>
        /// <returns>any ulong, all 64 bits are pseudo-random</returns>
        public ulong NextULong()
        {
            ulong s = (State += 0x6C8E9CF570932BD5UL);
            s = (s ^ (s >> 25)) * (s | 0xA529UL);
            return s ^ (s >> 22);
        }

        public static ulong Determine(ulong state)
        {
            return (state = ((state *= 0x6C8E9CF570932BD5UL) ^ (state >> 25)) * (state | 0xA529UL)) ^ (state >> 22);
        }

        /// <summary>
        /// Gets a random int that is between 0 (inclusive) and maxValue (exclusive), which must be
        /// positive (if it is 1 or less, this simply returns 0).
        /// </summary>
        /// <remarks>Credit to user craigster0 from https://stackoverflow.com/a/28904636 for the code this uses, which gets the high 64 bits of a 64-by-64-bit multiplication.</remarks>
        /// <param name="maxValue">the exclusive upper bound, which should be 1 or greater</param>
        /// <returns>a pseudo-random long between 0 (inclusive) and maxValue (exclusive)</returns>

        public long NextLong(long maxValue)
        {
            if (maxValue <= 1L)
                return 0L;
            unchecked
            {
                ulong a = NextULong();
                ulong a_lo = a & 0xFFFFFFFFUL;
                ulong a_hi = a >> 32;
                ulong b_lo = (ulong)maxValue & 0xFFFFFFFFUL;
                ulong b_hi = (ulong)maxValue >> 32;

                ulong a_x_b_hi = a_hi * b_hi;
                ulong a_x_b_mid = a_hi * b_lo;
                ulong b_x_a_mid = b_hi * a_lo;
                ulong a_x_b_lo = a_lo * b_lo;

                ulong carry_bit = ((a_x_b_mid & 0xFFFFFFFFUL) +
                                   (b_x_a_mid & 0xFFFFFFFFUL) +
                                   (a_x_b_lo >> 32)) >> 32;

                return (long)(a_x_b_hi + (a_x_b_mid >> 32) + (b_x_a_mid >> 32) + carry_bit);
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
        /// Returns a positive pseudo-random int, which can have any 31-bit positive value.
        /// </summary>
        /// <returns>any random positive int, all but the sign bit are pseudo-random</returns>
        public override int Next()
        {
            ulong s = (State += 0x6C8E9CF570932BD5UL);
            s = (s ^ (s >> 25)) * (s | 0xA529UL);
            return (int)(s ^ (s >> 22)) & 0x7fffffff;
        }
        /// <summary>
        /// Gets a random int that is between 0 (inclusive) and maxValue (exclusive), which can be positive or negative.
        /// </summary>
        /// <remarks>Based on code by Daniel Lemire, http://lemire.me/blog/2016/06/27/a-fast-alternative-to-the-modulo-reduction/ </remarks>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public override int Next(int maxValue)
        {
            ulong s = (State += 0x6C8E9CF570932BD5UL);
            s = (s ^ (s >> 25)) * (s | 0xA529UL);
            return (int)(((ulong)maxValue * ((s ^ (s >> 22)) & 0x7FFFFFFFL)) >> 31);
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
        /// Based on reference code in the documentation for java.util.Random.
        /// </remarks>
        /// <param name="buffer">a non-null byte array that will be modified</param>
        public override void NextBytes(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            for (int i = 0; i < buffer.Length;)
            {
                ulong s = (State += 0x6C8E9CF570932BD5UL);
                s = (s ^ (s >> 25)) * (s | 0xA529UL);
                s ^= (s >> 22);
                for (int n = Math.Min(buffer.Length - i, 8); n-- > 0; s >>= 8)
                    buffer[i++] = (byte)s;
            }
        }
        /// <summary>
        /// Gets a random double between 0.0 (inclusive) and 1.0 (exclusive).
        /// </summary
        /// <returns>a pseudo-random double between 0.0 inclusive and 1.0 exclusive</returns>
        public override double NextDouble()
        {
            ulong s = (State += 0x6C8E9CF570932BD5UL);
            s = (s ^ (s >> 25)) * (s | 0xA529UL);
            return (((s ^ (s >> 22))) & 0x1FFFFFFFFFFFFFUL) * 1.1102230246251565E-16;
        }
        /// <summary>
        /// Gets a random double between -1.0 (exclusive) and 1.0 (exclusive).
        /// </summary>
        /// <returns>a pseudo-random double between -1.0 exclusive and 1.0 exclusive</returns>
        public double NextSignedDouble()
        {
            ulong s = (State += 0x6C8E9CF570932BD5UL);
            s = (s ^ (s >> 25)) * (s | 0xA529UL);
            return ((long)((s ^ (s >> 22))) >> 11) * 1.1102230246251565E-16;
        }
        /// <summary>
        /// Gets a random double between 0.0 (inclusive) and 1.0 (exclusive).
        /// </summary>
        /// <remarks>
        /// The same code as NextDouble().
        /// </remarks>
        /// <returns>a pseudo-random double between 0.0 inclusive and 1.0 exclusive</returns>
        protected override double Sample()
        {
            ulong s = (State += 0x6C8E9CF570932BD5UL);
            s = (s ^ (s >> 25)) * (s | 0xA529UL);
            return (((s ^ (s >> 22))) & 0x1FFFFFFFFFFFFFUL) * 1.1102230246251565E-16;
        }
        /// <summary>
        /// Returns a new TAPRNG using the same algorithm and a copy of the internal state this uses.
        /// Calling the same methods on this TAPRNG and its copy should produce the same values.
        /// </summary>
        /// <returns>a copy of this TAPRNG</returns>
        public TAPRNG Copy()
        {
            return new TAPRNG(State);
        }
    }
    /// <summary>
    /// Not recommended; speed is mediocre compared to TAPRNG or PRNG3, and while the period is fairly good (at least 2 to the 64, possibly much higher), the statistical quality isn't as good.
    /// Quality starts off very high but trails to "mildly suspicious" before TAPRNG does, at 32TB (TAPRNG doesn't have any suspicious results in 32TB of testing with PractRand).
    /// This is often a sign that the generator will fail soon after.
    /// </summary>
    public class PRNG6 : Random
    {
        public ulong A, B, C, D;

        public PRNG6()
            : this((ulong)RNG.GlobalRandom.Next() >> 5 ^ (ulong)RNG.GlobalRandom.Next() << 21 ^ (ulong)RNG.GlobalRandom.Next() << 42)
        {
        }

        public PRNG6(ulong state)
        {
            A = (state + 0x9E3779B97F4A7C15UL);
            B = (A + 0x6C8E9CD570932BD5UL);
            C = (B + 0x6C8E9CD570932BD5UL);
            D = (C + 0x6C8E9CD570932BD5UL);
            B = (B ^ (B >> 25)) * (B | 0xA529UL);
            B ^= (B >> 22);
            C = (C ^ (C >> 25)) * (C | 0xA529UL);
            C ^= (C >> 22);
            D += (A & B) + (A & C) + (B & C);
        }

        public PRNG6(ulong a, ulong b, ulong c, ulong d)
        {
            A = a;
            B = b;
            C = c;
            D = d;
        }

        public void FromSnapshot(byte[] snapshot)
        {
            if (snapshot == null)
                throw new ArgumentNullException("snapshot");
            if (snapshot.Length < 32)
            {
                A = (ulong)(-0x3943D8696D4A3B7DL - snapshot.LongLength * 0x7CD6391461952C1DL);
                B = (ulong)(0x7CD6391461952C1DL + snapshot.LongLength * 0x3943D8696D4A3B7DL);
                C = (ulong)(0x3943D8696D4A3B7DL + snapshot.LongLength * -0x7CD6391461952C1DL);
                D = (ulong)(-0x7CD6391461952C1DL - snapshot.LongLength * -0x3943D8696D4A3B7DL);
            }
            else
            {
                A = BitConverter.ToUInt64(snapshot, 0);
                B = BitConverter.ToUInt64(snapshot, 8);
                C = BitConverter.ToUInt64(snapshot, 16);
                D = BitConverter.ToUInt64(snapshot, 24);
            }
        }

        public byte[] GetSnapshot()
        {
            byte[] snap = new byte[32];
            Buffer.BlockCopy(new ulong[] { A, B, C, D }, 0, snap, 0, 32);
            return snap;
        }

        /// <summary>
        /// Returns a pseudo-random int, which can be positive or negative and have any 32-bit value.
        /// </summary>
        /// <returns>any int, all 32 bits are pseudo-random</returns>
        public int NextInt()
        {
            A += B;
            B -= C;
            C += A;
            A ^= D++;
            C.Rol48();
            return (int)A;
        }
        /// <summary>
        /// Returns a pseudo-random long, which can be positive or negative and have any 64-bit value.
        /// </summary>
        /// <returns>any long, all 64 bits are pseudo-random</returns>
        public long NextLong()
        {
            A += B;
            B -= C;
            C += A;
            A ^= D++;
            C.Rol48();
            return (long)A;
        }
        /// <summary>
        /// Returns a pseudo-random unsigned long, which can have any 64-bit value.
        /// </summary>
        /// <returns>any ulong, all 64 bits are pseudo-random</returns>
        public ulong NextULong()
        {
            A += B;
            B -= C;
            C += A;
            A ^= D++;
            C.Rol48();
            return A;
        }

        /// <summary>
        /// Gets a random int that is between 0 (inclusive) and maxValue (exclusive), which must be
        /// positive (if it is 1 or less, this simply returns 0).
        /// </summary>
        /// <remarks>Credit to user craigster0 from https://stackoverflow.com/a/28904636 for the code this uses, which gets the high 64 bits of a 64-by-64-bit multiplication.</remarks>
        /// <param name="maxValue">the exclusive upper bound, which should be 1 or greater</param>
        /// <returns>a pseudo-random long between 0 (inclusive) and maxValue (exclusive)</returns>

        public long NextLong(long maxValue)
        {
            if (maxValue <= 1L)
                return 0L;
            unchecked
            {
                ulong a = NextULong();
                ulong a_lo = a & 0xFFFFFFFFUL;
                ulong a_hi = a >> 32;
                ulong b_lo = (ulong)maxValue & 0xFFFFFFFFUL;
                ulong b_hi = (ulong)maxValue >> 32;

                ulong a_x_b_hi = a_hi * b_hi;
                ulong a_x_b_mid = a_hi * b_lo;
                ulong b_x_a_mid = b_hi * a_lo;
                ulong a_x_b_lo = a_lo * b_lo;

                ulong carry_bit = ((a_x_b_mid & 0xFFFFFFFFUL) +
                                   (b_x_a_mid & 0xFFFFFFFFUL) +
                                   (a_x_b_lo >> 32)) >> 32;

                return (long)(a_x_b_hi + (a_x_b_mid >> 32) + (b_x_a_mid >> 32) + carry_bit);
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
        /// Returns a positive pseudo-random int, which can have any 31-bit positive value.
        /// </summary>
        /// <returns>any random positive int, all but the sign bit are pseudo-random</returns>
        public override int Next()
        {
            A += B;
            B -= C;
            C += A;
            A ^= D++;
            C.Rol48();
            return (int)A & 0x7fffffff;
        }
        /// <summary>
        /// Gets a random int that is between 0 (inclusive) and maxValue (exclusive), which can be positive or negative.
        /// </summary>
        /// <remarks>Based on code by Daniel Lemire, http://lemire.me/blog/2016/06/27/a-fast-alternative-to-the-modulo-reduction/ </remarks>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public override int Next(int maxValue)
        {
            A += B;
            B -= C;
            C += A;
            A ^= D++;
            C.Rol48();
            return (int)(((ulong)maxValue * (A & 0x7FFFFFFFL)) >> 31);
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
        /// Based on reference code in the documentation for java.util.Random.
        /// </remarks>
        /// <param name="buffer">a non-null byte array that will be modified</param>
        public override void NextBytes(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            ulong s;
            for (int i = 0; i < buffer.Length;)
            {
                A += B;
                B -= C;
                C += A;
                A ^= D++;
                C.Rol48();
                s = A;
                for (int n = Math.Min(buffer.Length - i, 8); n-- > 0; s >>= 8)
                    buffer[i++] = (byte)s;
            }
        }
        /// <summary>
        /// Gets a random double between 0.0 (inclusive) and 1.0 (exclusive).
        /// </summary
        /// <returns>a pseudo-random double between 0.0 inclusive and 1.0 exclusive</returns>
        public override double NextDouble()
        {
            A += B;
            B -= C;
            C += A;
            A ^= D++;
            C.Rol48();
            return (A & 0x1FFFFFFFFFFFFFUL) * 1.1102230246251565E-16;
        }
        /// <summary>
        /// Gets a random double between -1.0 (exclusive) and 1.0 (exclusive).
        /// </summary>
        /// <returns>a pseudo-random double between -1.0 exclusive and 1.0 exclusive</returns>
        public double NextSignedDouble()
        {
            A += B;
            B -= C;
            C += A;
            A ^= D++;
            C.Rol48();
            return ((long)A >> 11) * 1.1102230246251565E-16;
        }
        /// <summary>
        /// Gets a random double between 0.0 (inclusive) and 1.0 (exclusive).
        /// </summary>
        /// <remarks>
        /// The same code as NextDouble().
        /// </remarks>
        /// <returns>a pseudo-random double between 0.0 inclusive and 1.0 exclusive</returns>
        protected override double Sample()
        {
            A += B;
            B -= C;
            C += A;
            A ^= D++;
            C.Rol48();
            return (A & 0x1FFFFFFFFFFFFFUL) * 1.1102230246251565E-16;
        }
        /// <summary>
        /// Returns a new PRNG6 using the same algorithm and a copy of the internal state this uses.
        /// Calling the same methods on this PRNG6 and its copy should produce the same values.
        /// </summary>
        /// <returns>a copy of this PRNG6</returns>
        public PRNG6 Copy()
        {
            return new PRNG6(A, B, C, D);
        }
    }
    /// <summary>
    /// An unusual PRNG that mixes xoroshiro's algorithm (modified for 32-bit) and also incorporates a large-increment counter, with 96 bits of state.
    /// It passes 32TB of testing in PractRand (unmodified xoroshiro fails even with 128 bits of state), and has a period of 0xFFFFFFFFFFFFFFFF00000000
    /// (79228162514264337589248983040), or (2 to the 64 minus 1) times (2 to the 32). It does not offer a skip-ahead or skip-behind method.
    /// </summary>
    /// <remarks>
    /// It currently isn't as fast as it should be on x86 targets, but is likely to improve if .NET Core 2 or later can be used (or some other version
    /// of the CLR that uses RyuJIT on x86). This is because RyuJIT can optimize bitwise rotations (also called cyclic shifts or barrel shifts) into as
    /// little as one SSE instruction, but the older JIT32 compiler can only treat them as two bitwise shifts and a bitwise or, which wrecks the
    /// performance of OriolePRNG when JIT32 is used relative to RyuJIT.
    /// </remarks>
    public class OriolePRNG : Random
    {
        public uint A, B, C;

        public OriolePRNG()
            : this((uint)RNG.GlobalRandom.Next() >> 5 ^ (uint)RNG.GlobalRandom.Next() << 17,
                  (uint)RNG.GlobalRandom.Next() >> 6 ^ (uint)RNG.GlobalRandom.Next() << 16,
                  (uint)RNG.GlobalRandom.Next() >> 7 ^ (uint)RNG.GlobalRandom.Next() << 15)
        {
        }

        public OriolePRNG(ulong state)
        {
            A = (uint)(state & 0xFFFFFFFFUL);
            B = (uint)(state >> 32);
            uint z = ((A ^ B) + 0x9E3779B9U);//(State == 0U) ? (Inc += 0x632BE5A6U) : Inc);
            z = (z ^ (z >> 16)) * 0x85EBCA6BU;
            z = (z ^ (z >> 13)) * 0xC2B2AE35U;
            C = (z ^ (z >> 16));
        }

        public OriolePRNG(uint a, uint b, uint c)
        {
            A = a;
            B = b;
            C = c;
        }

        public void FromSnapshot(byte[] snapshot)
        {
            if (snapshot == null)
                throw new ArgumentNullException("snapshot");
            if (snapshot.Length < 12)
            {
                uint seed2 = Determine((uint)snapshot.Length);
                A = Determine(seed2);
                B = Determine(seed2 + 1);
                C = Determine(seed2 + 2);
            }
            else
            {
                A = BitConverter.ToUInt32(snapshot, 0);
                B = BitConverter.ToUInt32(snapshot, 4);
                C = BitConverter.ToUInt32(snapshot, 8);
            }
        }

        public byte[] GetSnapshot()
        {
            byte[] snap = new byte[12];
            Buffer.BlockCopy(new uint[] { A, B, C }, 0, snap, 0, 12);
            return snap;
        }

        /// <summary>
        /// Returns a pseudo-random int, which can be positive or negative and have any 32-bit value.
        /// </summary>
        /// <returns>any int, all 32 bits are pseudo-random</returns>
        public int NextInt()
        {
            uint s0 = A;
            uint s1 = B;
            uint result = s0 + s1;
            s1 ^= s0;
            A = (s0 << 13 | s0 >> 19) ^ s1 ^ (s1 << 5);
            B = (s1 << 28 | s1 >> 4);
            return (int)((result << 29 | result >> 3) + (C += 0x632BE5ABU));
        }
        /// <summary>
        /// Returns a pseudo-random long, which can be positive or negative and have any 64-bit value.
        /// </summary>
        /// <returns>any long, all 64 bits are pseudo-random</returns>
        public long NextLong()
        {
            uint s0 = A;
            uint s1 = B;
            uint high = s0 + s1;
            s1 ^= s0;
            uint s00 = (s0 << 13 | s0 >> 19) ^ s1 ^ (s1 << 5);
            s1 = (s1 << 28 | s1 >> 4);
            uint low = s00 + s1;
            s1 ^= s00;
            A = (s00 << 13 | s00 >> 19) ^ s1 ^ (s1 << 5);
            B = (s1 << 28 | s1 >> 4);
            return (long)((high << 29 | high >> 3) + (C + 0x632BE5ABU)) << 32 ^ ((low << 29 | low >> 3) + (C += 0xC657CB56U));
        }
        /// <summary>
        /// Returns a pseudo-random unsigned long, which can have any 64-bit value.
        /// </summary>
        /// <returns>any ulong, all 64 bits are pseudo-random</returns>
        public ulong NextULong()
        {
            uint s0 = A;
            uint s1 = B;
            uint high = s0 + s1;
            s1 ^= s0;
            uint s00 = (s0 << 13 | s0 >> 19) ^ s1 ^ (s1 << 5);
            s1 = (s1 << 28 | s1 >> 4);
            uint low = s00 + s1;
            s1 ^= s00;
            A = (s00 << 13 | s00 >> 19) ^ s1 ^ (s1 << 5);
            B = (s1 << 28 | s1 >> 4);
            return (ulong)((high << 29 | high >> 3) + (C + 0x632BE5ABU)) << 32 ^ ((low << 29 | low >> 3) + (C += 0xC657CB56U));
        }

        /// <summary>
        /// Gets a random int that is between 0 (inclusive) and maxValue (exclusive), which must be
        /// positive (if it is 1 or less, this simply returns 0).
        /// </summary>
        /// <remarks>Credit to user craigster0 from https://stackoverflow.com/a/28904636 for the code this uses, which gets the high 64 bits of a 64-by-64-bit multiplication.</remarks>
        /// <param name="maxValue">the exclusive upper bound, which should be 1 or greater</param>
        /// <returns>a pseudo-random long between 0 (inclusive) and maxValue (exclusive)</returns>

        public long NextLong(long maxValue)
        {
            if (maxValue <= 1L)
                return 0L;
            unchecked
            {
                ulong a = NextULong();
                ulong a_lo = a & 0xFFFFFFFFUL;
                ulong a_hi = a >> 32;
                ulong b_lo = (ulong)maxValue & 0xFFFFFFFFUL;
                ulong b_hi = (ulong)maxValue >> 32;

                ulong a_x_b_hi = a_hi * b_hi;
                ulong a_x_b_mid = a_hi * b_lo;
                ulong b_x_a_mid = b_hi * a_lo;
                ulong a_x_b_lo = a_lo * b_lo;

                ulong carry_bit = ((a_x_b_mid & 0xFFFFFFFFUL) +
                                   (b_x_a_mid & 0xFFFFFFFFUL) +
                                   (a_x_b_lo >> 32)) >> 32;

                return (long)(a_x_b_hi + (a_x_b_mid >> 32) + (b_x_a_mid >> 32) + carry_bit);
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
        /// Returns a positive pseudo-random int, which can have any 31-bit positive value.
        /// </summary>
        /// <returns>any random positive int, all but the sign bit are pseudo-random</returns>
        public override int Next()
        {
            uint s0 = A;
            uint s1 = B;
            uint result = s0 + s1;
            s1 ^= s0;
            A = (s0 << 13 | s0 >> 19) ^ s1 ^ (s1 << 5);
            B = (s1 << 28 | s1 >> 4);
            return (int)((result << 29 | result >> 3) + (C += 0x632BE5ABU)) & 0x7fffffff;
        }
        /// <summary>
        /// Gets a random int that is between 0 (inclusive) and maxValue (exclusive), which can be positive or negative.
        /// </summary>
        /// <remarks>Based on code by Daniel Lemire, http://lemire.me/blog/2016/06/27/a-fast-alternative-to-the-modulo-reduction/ </remarks>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public override int Next(int maxValue)
        {
            uint s0 = A;
            uint s1 = B;
            uint result = s0 + s1;
            s1 ^= s0;
            A = (s0 << 13 | s0 >> 19) ^ s1 ^ (s1 << 5);
            B = (s1 << 28 | s1 >> 4);
            return (int)(((ulong)maxValue * ((((result << 29 | result >> 3) + (C += 0x632BE5ABU)) & 0x7FFFFFFFUL))) >> 31);
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
        /// Based on reference code in the documentation for java.util.Random.
        /// </remarks>
        /// <param name="buffer">a non-null byte array that will be modified</param>
        public override void NextBytes(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            uint s;
            for (int i = 0; i < buffer.Length;)
            {
                uint s0 = A;
                uint s1 = B;
                uint result = s0 + s1;
                s1 ^= s0;
                A = (s0 << 13 | s0 >> 19) ^ s1 ^ (s1 << 5);
                B = (s1 << 28 | s1 >> 4);
                s = (result << 29 | result >> 3) + (C += 0x632BE5ABU);
                for (int n = Math.Min(buffer.Length - i, 4); n-- > 0; s >>= 4)
                    buffer[i++] = (byte)s;
            }
        }
        /// <summary>
        /// Gets a random double between 0.0 (inclusive) and 1.0 (exclusive).
        /// </summary
        /// <returns>a pseudo-random double between 0.0 inclusive and 1.0 exclusive</returns>
        public override double NextDouble()
        {
            uint s0 = A;
            uint s1 = B;
            uint high = s0 + s1;
            s1 ^= s0;
            uint s00 = (s0 << 13 | s0 >> 19) ^ s1 ^ (s1 << 5);
            s1 = (s1 << 28 | s1 >> 4);
            uint low = s00 + s1;
            s1 ^= s00;
            A = (s00 << 13 | s00 >> 19) ^ s1 ^ (s1 << 5);
            B = (s1 << 28 | s1 >> 4);
            return (((ulong)((high << 29 | high >> 3) + (C + 0x632BE5ABU)) << 32 ^ ((low << 29 | low >> 3) + (C += 0xC657CB56U))) & 0x1FFFFFFFFFFFFFUL) * 1.1102230246251565E-16;
        }
        /// <summary>
        /// Gets a random double between -1.0 (exclusive) and 1.0 (exclusive).
        /// </summary>
        /// <returns>a pseudo-random double between -1.0 exclusive and 1.0 exclusive</returns>
        public double NextSignedDouble()
        {
            uint s0 = A;
            uint s1 = B;
            uint high = s0 + s1;
            s1 ^= s0;
            uint s00 = (s0 << 13 | s0 >> 19) ^ s1 ^ (s1 << 5);
            s1 = (s1 << 28 | s1 >> 4);
            uint low = s00 + s1;
            s1 ^= s00;
            A = (s00 << 13 | s00 >> 19) ^ s1 ^ (s1 << 5);
            B = (s1 << 28 | s1 >> 4);
            return (((long)((high << 29 | high >> 3) + (C + 0x632BE5ABU)) << 32 ^ ((low << 29 | low >> 3) + (C += 0xC657CB56U))) >> 11) * 1.1102230246251565E-16;
        }
        /// <summary>
        /// Gets a random double between 0.0 (inclusive) and 1.0 (exclusive).
        /// </summary>
        /// <remarks>
        /// The same code as NextDouble().
        /// </remarks>
        /// <returns>a pseudo-random double between 0.0 inclusive and 1.0 exclusive</returns>
        protected override double Sample()
        {
            uint s0 = A;
            uint s1 = B;
            uint high = s0 + s1;
            s1 ^= s0;
            uint s00 = (s0 << 13 | s0 >> 19) ^ s1 ^ (s1 << 5);
            s1 = (s1 << 28 | s1 >> 4);
            uint low = s00 + s1;
            s1 ^= s00;
            A = (s00 << 13 | s00 >> 19) ^ s1 ^ (s1 << 5);
            B = (s1 << 28 | s1 >> 4);
            return (((ulong)((high << 29 | high >> 3) + (C + 0x632BE5ABU)) << 32 ^ ((low << 29 | low >> 3) + (C += 0xC657CB56U))) & 0x1FFFFFFFFFFFFFUL) * 1.1102230246251565E-16;
        }
        /// <summary>
        /// Returns a new PRNG7 using the same algorithm and a copy of the internal state this uses.
        /// Calling the same methods on this PRNG7 and its copy should produce the same values.
        /// </summary>
        /// <returns>a copy of this PRNG7</returns>
        public OriolePRNG Copy()
        {
            return new OriolePRNG(A, B, C);
        }
        /// <summary>
        /// Given any uint called state, this produces a unique uint that should seem to have no relation to state.
        /// </summary>
        /// <param name="state">any uint</param>
        /// <returns>any uint</returns>
        public static uint Determine(uint state)
        {
            state = ((state *= 0x9E3779B9U) ^ (state >> 16)) * 0x85EBCA6BU;
            state = (state ^ (state >> 13)) * 0xC2B2AE35U;
            return state ^ (state >> 16);
        }
    }
    /// <summary>
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class LinnormPRNG : Random
    {
        public ulong State;

        public LinnormPRNG()
            : this((ulong)RNG.GlobalRandom.Next() >> 5 ^ (ulong)RNG.GlobalRandom.Next() << 21 ^ (ulong)RNG.GlobalRandom.Next() << 42)
        {
        }

        public LinnormPRNG(ulong state)
        {
            State = state;
        }

        public void FromSnapshot(byte[] snapshot)
        {
            if (snapshot == null)
                throw new ArgumentNullException("snapshot");
            if (snapshot.Length < 8)
                State = (ulong)(-1L - snapshot.LongLength * 421L);
            else
                State = BitConverter.ToUInt64(snapshot, 0);
        }

        public byte[] GetSnapshot()
        {
            return BitConverter.GetBytes(State);
        }
        /// <summary>
        /// Returns a pseudo-random int, which can be positive or negative and have any 32-bit value.
        /// </summary>
        /// <returns>any int, all 32 bits are pseudo-random</returns>
        public int NextInt()
        {
            ulong z = (State = State * 0x41C64E6DUL + 1UL);
            z = (z ^ z >> 27) * 0xAEF17502108EF2D9UL;
            return (int)(z ^ z >> 25);

        }
        /// <summary>
        /// Returns a pseudo-random long, which can be positive or negative and have any 64-bit value.
        /// </summary>
        /// <returns>any long, all 64 bits are pseudo-random</returns>
        public long NextLong()
        {
            ulong z = (State = State * 0x41C64E6DUL + 1UL);
            z = (z ^ z >> 27) * 0xAEF17502108EF2D9UL;
            return (long)(z ^ z >> 25);
        }
        /// <summary>
        /// Returns a pseudo-random unsigned long, which can have any 64-bit value.
        /// </summary>
        /// <returns>any ulong, all 64 bits are pseudo-random</returns>
        public ulong NextULong()
        {
            ulong z = (State = State * 0x41C64E6DUL + 1UL);
            z = (z ^ z >> 27) * 0xAEF17502108EF2D9UL;
            return z ^ z >> 25;
        }

        public static ulong Determine(ulong state)
        {
            return (state = ((state = (((state * 0x632BE59BD9B4E019UL) ^ 0x9E3779B97F4A7C15UL) * 0xC6BC279692B5CC83UL)) ^ state >> 27) * 0xAEF17502108EF2D9UL) ^ state >> 25;
        }

        /// <summary>
        /// Gets a random int that is between 0 (inclusive) and maxValue (exclusive), which must be
        /// positive (if it is 1 or less, this simply returns 0).
        /// </summary>
        /// <remarks>Credit to user craigster0 from https://stackoverflow.com/a/28904636 for the code this uses, which gets the high 64 bits of a 64-by-64-bit multiplication.</remarks>
        /// <param name="maxValue">the exclusive upper bound, which should be 1 or greater</param>
        /// <returns>a pseudo-random long between 0 (inclusive) and maxValue (exclusive)</returns>

        public long NextLong(long maxValue)
        {
            if (maxValue <= 1L)
                return 0L;
            unchecked
            {
                ulong a = NextULong();
                ulong a_lo = a & 0xFFFFFFFFUL;
                ulong a_hi = a >> 32;
                ulong b_lo = (ulong)maxValue & 0xFFFFFFFFUL;
                ulong b_hi = (ulong)maxValue >> 32;

                ulong a_x_b_hi = a_hi * b_hi;
                ulong a_x_b_mid = a_hi * b_lo;
                ulong b_x_a_mid = b_hi * a_lo;
                ulong a_x_b_lo = a_lo * b_lo;

                ulong carry_bit = ((a_x_b_mid & 0xFFFFFFFFUL) +
                                   (b_x_a_mid & 0xFFFFFFFFUL) +
                                   (a_x_b_lo >> 32)) >> 32;

                return (long)(a_x_b_hi + (a_x_b_mid >> 32) + (b_x_a_mid >> 32) + carry_bit);
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
        /// Returns a positive pseudo-random int, which can have any 31-bit positive value.
        /// </summary>
        /// <returns>any random positive int, all but the sign bit are pseudo-random</returns>
        public override int Next()
        {
            ulong z = (State = State * 0x41C64E6DUL + 1UL);
            z = (z ^ z >> 27) * 0xAEF17502108EF2D9UL;
            return (int)(z ^ z >> 25) & 0x7fffffff;
        }
        /// <summary>
        /// Gets a random int that is between 0 (inclusive) and maxValue (exclusive), which can be positive or negative.
        /// </summary>
        /// <remarks>Based on code by Daniel Lemire, http://lemire.me/blog/2016/06/27/a-fast-alternative-to-the-modulo-reduction/ </remarks>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public override int Next(int maxValue)
        {
            return (int)(((ulong)maxValue * (NextULong() & 0x7FFFFFFFUL)) >> 31);
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
        /// Based on reference code in the documentation for java.util.Random.
        /// </remarks>
        /// <param name="buffer">a non-null byte array that will be modified</param>
        public override void NextBytes(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            for (int i = 0; i < buffer.Length;)
            {
                ulong s = NextULong();
                for (int n = Math.Min(buffer.Length - i, 8); n-- > 0; s >>= 8)
                    buffer[i++] = (byte)s;
            }
        }
        /// <summary>
        /// Gets a random double between 0.0 (inclusive) and 1.0 (exclusive).
        /// </summary
        /// <returns>a pseudo-random double between 0.0 inclusive and 1.0 exclusive</returns>
        public override double NextDouble()
        {
            return (NextULong() & 0x1FFFFFFFFFFFFFUL) * 1.1102230246251565E-16;
        }
        /// <summary>
        /// Gets a random double between -1.0 (exclusive) and 1.0 (exclusive).
        /// </summary>
        /// <returns>a pseudo-random double between -1.0 exclusive and 1.0 exclusive</returns>
        public double NextSignedDouble()
        {
            return (NextLong() >> 11) * 1.1102230246251565E-16;
        }
        /// <summary>
        /// Gets a random double between 0.0 (inclusive) and 1.0 (exclusive).
        /// </summary>
        /// <remarks>
        /// The same code as NextDouble().
        /// </remarks>
        /// <returns>a pseudo-random double between 0.0 inclusive and 1.0 exclusive</returns>
        protected override double Sample()
        {
            return (NextULong() & 0x1FFFFFFFFFFFFFUL) * 1.1102230246251565E-16;
        }
        /// <summary>
        /// Returns a new LinnormPRNG using the same algorithm and a copy of the internal state this uses.
        /// Calling the same methods on this LinnormPRNG and its copy should produce the same values.
        /// </summary>
        /// <returns>a copy of this LinnormPRNG</returns>
        public LinnormPRNG Copy()
        {
            return new LinnormPRNG(State);
        }
    }
    /// <summary>
    /// A 32-bit class that uses a mix of ThrustAlt's algorithm with a Marsaglia triple-XorShift generator to get higher quality with 32-bit math.
    /// This has a period of <code>Math.Pow(2, 64) - Math.Pow(2, 32)</code>, and will produce each 32-bit result <code>Math.Pow(2, 32) - 1</code>
    /// times over the course of its period.
    /// </summary>
    public class XTPRNG : Random
    {
        public uint State, Stream;

        public XTPRNG()
            : this(RNG.GlobalRandom.Next() >> 5 ^ RNG.GlobalRandom.Next() << 11, RNG.GlobalRandom.Next() >> 5 ^ RNG.GlobalRandom.Next() << 11)
        {
        }

        public XTPRNG(int state)
        {
            State = (uint)state;
            Stream = (State ^ 0xC74EAD55u) * 0x947E3DB3u;
            if (Stream == 0) Stream = 1u;
        }

        public XTPRNG(int state, int stream)
        {
            State = (uint)state;
            if (stream == 0) Stream = 1u;
            else Stream = (uint)stream;
        }

        public XTPRNG(uint state, uint stream)
        {
            State = state;
            if (stream == 0u) Stream = 1u;
            else Stream = stream;
        }

        public void FromSnapshot(byte[] snapshot)
        {
            if (snapshot == null)
                throw new ArgumentNullException("snapshot");
            if (snapshot.Length < 8)
            {
                State = (uint)(-1L - snapshot.Length * 421);
                Stream = State << 3 | 5u;
            }
            else
            {
                State = BitConverter.ToUInt32(snapshot, 0);
                Stream = BitConverter.ToUInt32(snapshot, 4);
            }
        }

        public byte[] GetSnapshot()
        {
            return
                (BitConverter.IsLittleEndian)
                ? BitConverter.GetBytes(State | ((ulong)Stream << 32))
                : BitConverter.GetBytes(Stream | ((ulong)State << 32));
        }
        /// <summary>
        /// Returns a pseudo-random int, which can be positive or negative and have any 32-bit value.
        /// </summary>
        /// <returns>any int, all 32 bits are pseudo-random</returns>
        public int NextInt()
        {
            Stream ^= Stream >> 6;
            uint s = (State += 0x6C8E9CF5u);
            uint z = (s ^ (s >> 13)) * ((Stream ^= Stream << 1) | 1u);
            return (int)((z ^ z >> 11) + (Stream ^= Stream >> 11));
        }
        /// <summary>
        /// Returns a pseudo-random uint, which can have any unsigned 32-bit value.
        /// </summary>
        /// <returns>any uint, all 32 bits are pseudo-random</returns>
        public uint NextUInt()
        {
            Stream ^= Stream >> 6;
            uint s = (State += 0x6C8E9CF5u);
            uint z = (s ^ (s >> 13)) * ((Stream ^= Stream << 1) | 1u);
            return ((z ^ z >> 11) + (Stream ^= Stream >> 11));
        }
        /// <summary>
        /// Returns a pseudo-random long, which can be positive or negative and have any 64-bit value.
        /// </summary>
        /// <returns>any long, all 64 bits are pseudo-random</returns>
        public long NextLong()
        {
            Stream ^= Stream >> 6;
            uint t = (State += 0xD91D39EA), s = (t - 0x6C8E9CF5);
            uint z = (s ^ (s >> 13)) * ((Stream ^= Stream << 1) | 1);
            ulong hi = (z ^ z >> 11) + (Stream ^= Stream >> 11);
            Stream ^= Stream >> 6;
            uint lo = (t ^ (t >> 13)) * ((Stream ^= Stream << 1) | 1);
            return (long)(((lo ^ lo >> 11) + (Stream ^= Stream >> 11)) ^ hi << 32);
        }
        /// <summary>
        /// Returns a pseudo-random unsigned long, which can have any 64-bit value.
        /// </summary>
        /// <returns>any ulong, all 64 bits are pseudo-random</returns>
        public ulong NextULong()
        {
            Stream ^= Stream >> 6;
            uint t = (State += 0xD91D39EA), s = (t - 0x6C8E9CF5);
            uint z = (s ^ (s >> 13)) * ((Stream ^= Stream << 1) | 1);
            ulong hi = (z ^ z >> 11) + (Stream ^= Stream >> 11);
            Stream ^= Stream >> 6;
            uint lo = (t ^ (t >> 13)) * ((Stream ^= Stream << 1) | 1);
            return (((lo ^ lo >> 11) + (Stream ^= Stream >> 11)) ^ hi << 32);
        }


        /// <summary>
        /// Gets a random int that is between 0 (inclusive) and maxValue (exclusive), which must be
        /// positive (if it is 1 or less, this simply returns 0).
        /// </summary>
        /// <remarks>Credit to user craigster0 from https://stackoverflow.com/a/28904636 for the code this uses, which gets the high 64 bits of a 64-by-64-bit multiplication.</remarks>
        /// <param name="maxValue">the exclusive upper bound, which should be 1 or greater</param>
        /// <returns>a pseudo-random long between 0 (inclusive) and maxValue (exclusive)</returns>

        public long NextLong(long maxValue)
        {
            if (maxValue <= 1L)
                return 0L;
            unchecked
            {
                ulong a = NextULong();
                ulong a_lo = a & 0xFFFFFFFFUL;
                ulong a_hi = a >> 32;
                ulong b_lo = (ulong)maxValue & 0xFFFFFFFFUL;
                ulong b_hi = (ulong)maxValue >> 32;

                ulong a_x_b_hi = a_hi * b_hi;
                ulong a_x_b_mid = a_hi * b_lo;
                ulong b_x_a_mid = b_hi * a_lo;
                ulong a_x_b_lo = a_lo * b_lo;

                ulong carry_bit = ((a_x_b_mid & 0xFFFFFFFFUL) +
                                   (b_x_a_mid & 0xFFFFFFFFUL) +
                                   (a_x_b_lo >> 32)) >> 32;

                return (long)(a_x_b_hi + (a_x_b_mid >> 32) + (b_x_a_mid >> 32) + carry_bit);
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
        /// Returns a positive pseudo-random int, which can have any 31-bit positive value.
        /// </summary>
        /// <returns>any random positive int, all but the sign bit are pseudo-random</returns>
        public override int Next()
        {
            Stream ^= Stream >> 6;
            uint s = (State += 0x6C8E9CF5u);
            uint z = (s ^ (s >> 13)) * ((Stream ^= Stream << 1) | 1u);
            return (int)((z ^ z >> 11) + (Stream ^= Stream >> 11)) & 0x7fffffff;
        }
        /// <summary>
        /// Gets a random int that is between 0 (inclusive) and maxValue (exclusive), which can be positive or negative.
        /// </summary>
        /// <remarks>Based on code by Daniel Lemire, http://lemire.me/blog/2016/06/27/a-fast-alternative-to-the-modulo-reduction/ </remarks>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public override int Next(int maxValue)
        {
            return (int)(((ulong)maxValue * (NextUInt() & 0x7FFFFFFFUL)) >> 31);
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
        /// Based on reference code in the documentation for java.util.Random.
        /// </remarks>
        /// <param name="buffer">a non-null byte array that will be modified</param>
        public override void NextBytes(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            for (int i = 0; i < buffer.Length;)
            {
                uint s = NextUInt();
                for (int n = Math.Min(buffer.Length - i, 4); n-- > 0; s >>= 4)
                    buffer[i++] = (byte)s;
            }
        }
        /// <summary>
        /// Gets a random double between 0.0 (inclusive) and 1.0 (exclusive).
        /// </summary
        /// <returns>a pseudo-random double between 0.0 inclusive and 1.0 exclusive</returns>
        public override double NextDouble()
        {
            return (NextULong() & 0x1FFFFFFFFFFFFFUL) * 1.1102230246251565E-16;
        }
        /// <summary>
        /// Gets a random double between -1.0 (exclusive) and 1.0 (exclusive).
        /// </summary>
        /// <returns>a pseudo-random double between -1.0 exclusive and 1.0 exclusive</returns>
        public double NextSignedDouble()
        {
            return (NextLong() >> 11) * 1.1102230246251565E-16;
        }
        /// <summary>
        /// Gets a random double between 0.0 (inclusive) and 1.0 (exclusive).
        /// </summary>
        /// <remarks>
        /// The same code as NextDouble().
        /// </remarks>
        /// <returns>a pseudo-random double between 0.0 inclusive and 1.0 exclusive</returns>
        protected override double Sample()
        {
            return (NextULong() & 0x1FFFFFFFFFFFFFUL) * 1.1102230246251565E-16;
        }
        /// <summary>
        /// Returns a new XTPRNG using the same algorithm and a copy of the internal state this uses.
        /// Calling the same methods on this XTPRNG and its copy should produce the same values.
        /// </summary>
        /// <returns>a copy of this XTPRNG</returns>
        public XTPRNG Copy()
        {
            return new XTPRNG(State, Stream);
        }
    }
    /// <summary>
    /// A PRNG that uses xoroshiro's algorithm (modified for 32-bit) and adjusts its output with the "++" scrambler from Vigna and Blackman's paper introducing
    /// the xoshiro family of generators. This is the fastest generator here when producing 32-bit output, but it will lag behind LinnormPRNG on 64-bit output.
    /// It passes 32TB of testing in PractRand (xoroshiro128+ fails even with 128 bits of state), and has a period of 0xFFFFFFFFFFFFFFFF, or (2 to the 64 minus 1).
    /// It does not offer a skip-ahead or skip-behind method. It is fast on both x86 and x64 targets, in part because x64's RyuJIT can optimize it thoroughly.
    /// </summary>
    /// <remarks>
    /// It currently isn't as fast as it should be on x86 targets, but is likely to improve if .NET Core 2 or later can be used (or some other version
    /// of the CLR that uses RyuJIT on x86). This is because RyuJIT can optimize bitwise rotations (also called cyclic shifts or barrel shifts) into as
    /// little as one SSE instruction, but the older JIT32 compiler can only treat them as two bitwise shifts and a bitwise or, which worsens the
    /// performance of LathePRNG when JIT32 is used relative to RyuJIT. It's still the fastest (with high quality) without this optimization, though.
    /// </remarks>
    public class LathePRNG : Random
    {
        public uint A, B;

        public LathePRNG()
            : this((uint)RNG.GlobalRandom.Next() >> 5 ^ (uint)RNG.GlobalRandom.Next() << 17,
                  (uint)RNG.GlobalRandom.Next() >> 7 ^ (uint)RNG.GlobalRandom.Next() << 15)
        {
        }

        public LathePRNG(ulong state)
        {
            if (state == 0UL)
                A = 1U;
            else
                A = (uint)(state & 0xFFFFFFFFUL);
            B = (uint)(state >> 32);
        }

        public LathePRNG(uint a, uint b)
        {
            A = a;
            B = b;
        }

        public void FromSnapshot(byte[] snapshot)
        {
            if (snapshot == null)
                throw new ArgumentNullException("snapshot");
            if (snapshot.Length < 8)
            {
                A = (uint)(-1L - snapshot.Length * 421);
                B = Determine(A+1);
            }
            else
            {
                A = BitConverter.ToUInt32(snapshot, 0);
                B = BitConverter.ToUInt32(snapshot, 4);
            }
        }

        public void SetState(ulong state)
        {
            if (state == 0UL)
                A = 1u;
            else
                A = (uint)(state & 0xFFFFFFFFUL);
            B = (uint)(state >> 32);
        }
        public ulong GetState()
        {
            return A | ((ulong)B << 32);
        }

        public byte[] GetSnapshot()
        {
            return
                (BitConverter.IsLittleEndian)
                ? BitConverter.GetBytes(A | ((ulong)B << 32))
                : BitConverter.GetBytes(A | ((ulong)B << 32));
        }

        /// <summary>
        /// Returns a pseudo-random int, which can be positive or negative and have any 32-bit value.
        /// </summary>
        /// <returns>any int, all 32 bits are pseudo-random</returns>
        public int NextInt()
        {
            uint s0 = A;
            uint s1 = B;
            uint result = s0 + s1;
            s1 ^= s0;
            A = (s0 << 13 | s0 >> 19) ^ s1 ^ (s1 << 5);
            B = (s1 << 28 | s1 >> 4);
            return (int)((result << 10 | result >> 22) + s0);
        }
        /// <summary>
        /// Returns a pseudo-random uint, which can have any 32-bit value.
        /// </summary>
        /// <returns>any uint, all 32 bits are pseudo-random</returns>
        public uint NextUInt()
        {
            uint s0 = A;
            uint s1 = B;
            uint result = s0 + s1;
            s1 ^= s0;
            A = (s0 << 13 | s0 >> 19) ^ s1 ^ (s1 << 5);
            B = (s1 << 28 | s1 >> 4);
            return ((result << 10 | result >> 22) + s0);
        }
        /// <summary>
        /// Returns a pseudo-random long, which can be positive or negative and have any 64-bit value.
        /// </summary>
        /// <returns>any long, all 64 bits are pseudo-random</returns>
        public long NextLong()
        {
            uint s0 = A;
            uint s1 = B;
            uint high = s0 + s1;
            s1 ^= s0;
            uint s00 = (s0 << 13 | s0 >> 19) ^ s1 ^ (s1 << 5);
            s1 = (s1 << 28 | s1 >> 4);
            uint low = s00 + s1;
            s1 ^= s00;
            A = (s00 << 13 | s00 >> 19) ^ s1 ^ (s1 << 5);
            B = (s1 << 28 | s1 >> 4);
            return (long)((high << 10 | high >> 22) + s0) << 32 ^ ((low << 10 | low >> 22) + s00);
        }
        /// <summary>
        /// Returns a pseudo-random unsigned long, which can have any 64-bit value.
        /// </summary>
        /// <returns>any ulong, all 64 bits are pseudo-random</returns>
        public ulong NextULong()
        {
            uint s0 = A;
            uint s1 = B;
            uint high = s0 + s1;
            s1 ^= s0;
            uint s00 = (s0 << 13 | s0 >> 19) ^ s1 ^ (s1 << 5);
            s1 = (s1 << 28 | s1 >> 4);
            uint low = s00 + s1;
            s1 ^= s00;
            A = (s00 << 13 | s00 >> 19) ^ s1 ^ (s1 << 5);
            B = (s1 << 28 | s1 >> 4);
            return (ulong)((high << 10 | high >> 22) + s0) << 32 ^ ((low << 10 | low >> 22) + s00);
        }

        /// <summary>
        /// Gets a random int that is between 0 (inclusive) and maxValue (exclusive), which must be
        /// positive (if it is 1 or less, this simply returns 0).
        /// </summary>
        /// <remarks>Credit to user craigster0 from https://stackoverflow.com/a/28904636 for the code this uses, which gets the high 64 bits of a 64-by-64-bit multiplication.</remarks>
        /// <param name="maxValue">the exclusive upper bound, which should be 1 or greater</param>
        /// <returns>a pseudo-random long between 0 (inclusive) and maxValue (exclusive)</returns>

        public long NextLong(long maxValue)
        {
            if (maxValue <= 1L)
                return 0L;
            unchecked
            {
                ulong a = NextULong();
                ulong a_lo = a & 0xFFFFFFFFUL;
                ulong a_hi = a >> 32;
                ulong b_lo = (ulong)maxValue & 0xFFFFFFFFUL;
                ulong b_hi = (ulong)maxValue >> 32;

                ulong a_x_b_hi = a_hi * b_hi;
                ulong a_x_b_mid = a_hi * b_lo;
                ulong b_x_a_mid = b_hi * a_lo;
                ulong a_x_b_lo = a_lo * b_lo;

                ulong carry_bit = ((a_x_b_mid & 0xFFFFFFFFUL) +
                                   (b_x_a_mid & 0xFFFFFFFFUL) +
                                   (a_x_b_lo >> 32)) >> 32;

                return (long)(a_x_b_hi + (a_x_b_mid >> 32) + (b_x_a_mid >> 32) + carry_bit);
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
        /// Returns a positive pseudo-random int, which can have any 31-bit positive value.
        /// </summary>
        /// <returns>any random positive int, all but the sign bit are pseudo-random</returns>
        public override int Next()
        {
            uint s0 = A;
            uint s1 = B;
            uint result = s0 + s1;
            s1 ^= s0;
            A = (s0 << 13 | s0 >> 19) ^ s1 ^ (s1 << 5);
            B = (s1 << 28 | s1 >> 4);
            return (int)((result << 10 | result >> 22) + s0) & 0x7fffffff;
        }
        /// <summary>
        /// Gets a random int that is between 0 (inclusive) and maxValue (exclusive), which can be positive or negative.
        /// </summary>
        /// <remarks>Based on code by Daniel Lemire, http://lemire.me/blog/2016/06/27/a-fast-alternative-to-the-modulo-reduction/ </remarks>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public override int Next(int maxValue)
        {
            uint s0 = A;
            uint s1 = B;
            uint result = s0 + s1;
            s1 ^= s0;
            A = (s0 << 13 | s0 >> 19) ^ s1 ^ (s1 << 5);
            B = (s1 << 28 | s1 >> 4);
            return (int)(((ulong)maxValue * ((((result << 10 | result >> 22) + s0) & 0x7FFFFFFFUL))) >> 31);
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
        /// Based on reference code in the documentation for java.util.Random.
        /// </remarks>
        /// <param name="buffer">a non-null byte array that will be modified</param>
        public override void NextBytes(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            uint s;
            for (int i = 0; i < buffer.Length;)
            {
                uint s0 = A;
                uint s1 = B;
                uint result = s0 + s1;
                s1 ^= s0;
                A = (s0 << 13 | s0 >> 19) ^ s1 ^ (s1 << 5);
                B = (s1 << 28 | s1 >> 4);
                s = ((result << 10 | result >> 22) + s0);
                for (int n = Math.Min(buffer.Length - i, 4); n-- > 0; s >>= 4)
                    buffer[i++] = (byte)s;
            }
        }
        /// <summary>
        /// Gets a random double between 0.0 (inclusive) and 1.0 (exclusive).
        /// </summary
        /// <returns>a pseudo-random double between 0.0 inclusive and 1.0 exclusive</returns>
        public override double NextDouble()
        {
            uint s0 = A;
            uint s1 = B;
            uint high = s0 + s1;
            s1 ^= s0;
            uint s00 = (s0 << 13 | s0 >> 19) ^ s1 ^ (s1 << 5);
            s1 = (s1 << 28 | s1 >> 4);
            uint low = s00 + s1;
            s1 ^= s00;
            A = (s00 << 13 | s00 >> 19) ^ s1 ^ (s1 << 5);
            B = (s1 << 28 | s1 >> 4);
            return (((ulong)((high << 10 | high >> 22) + s0) << 32 ^ ((low << 10 | low >> 22) + s00)) & 0x1FFFFFFFFFFFFFUL) * 1.1102230246251565E-16;
        }
        /// <summary>
        /// Gets a random double between -1.0 (exclusive) and 1.0 (exclusive).
        /// </summary>
        /// <returns>a pseudo-random double between -1.0 exclusive and 1.0 exclusive</returns>
        public double NextSignedDouble()
        {
            uint s0 = A;
            uint s1 = B;
            uint high = s0 + s1;
            s1 ^= s0;
            uint s00 = (s0 << 13 | s0 >> 19) ^ s1 ^ (s1 << 5);
            s1 = (s1 << 28 | s1 >> 4);
            uint low = s00 + s1;
            s1 ^= s00;
            A = (s00 << 13 | s00 >> 19) ^ s1 ^ (s1 << 5);
            B = (s1 << 28 | s1 >> 4);
            return (((ulong)((high << 10 | high >> 22) + s0) << 32 ^ ((low << 10 | low >> 22) + s00)) >> 11) * 1.1102230246251565E-16;
        }
        /// <summary>
        /// Gets a random double between 0.0 (inclusive) and 1.0 (exclusive).
        /// </summary>
        /// <remarks>
        /// The same code as NextDouble().
        /// </remarks>
        /// <returns>a pseudo-random double between 0.0 inclusive and 1.0 exclusive</returns>
        protected override double Sample()
        {
            uint s0 = A;
            uint s1 = B;
            uint high = s0 + s1;
            s1 ^= s0;
            uint s00 = (s0 << 13 | s0 >> 19) ^ s1 ^ (s1 << 5);
            s1 = (s1 << 28 | s1 >> 4);
            uint low = s00 + s1;
            s1 ^= s00;
            A = (s00 << 13 | s00 >> 19) ^ s1 ^ (s1 << 5);
            B = (s1 << 28 | s1 >> 4);
            return (((ulong)((high << 10 | high >> 22) + s0) << 32 ^ ((low << 10 | low >> 22) + s00)) & 0x1FFFFFFFFFFFFFUL) * 1.1102230246251565E-16;
        }
        /// <summary>
        /// Returns a new PRNG7 using the same algorithm and a copy of the internal state this uses.
        /// Calling the same methods on this PRNG7 and its copy should produce the same values.
        /// </summary>
        /// <returns>a copy of this PRNG7</returns>
        public LathePRNG Copy()
        {
            return new LathePRNG(A, B);
        }
        /// <summary>
        /// Given any uint called state, this produces a unique uint that should seem to have no relation to state.
        /// </summary>
        /// <param name="state">any uint</param>
        /// <returns>any uint</returns>
        public static uint Determine(uint state)
        {
            state = ((state *= 0x9E3779B9U) ^ (state >> 16)) * 0x85EBCA6BU;
            state = (state ^ (state >> 13)) * 0xC2B2AE35U;
            return state ^ (state >> 16);
        }
    }

    public static class MathExtensions
    {
        public static void Rol48(ref this ulong ul) => ul = (ul << 48) | (ul >> 16);
        public static void Rotate13(ref this uint ul) => ul = (ul << 13) | (ul >> 19);
        public static void Rotate28(ref this uint ul) => ul = (ul << 28) | (ul >> 4);
        public static void Rotate29(ref this uint ul) => ul = (ul << 29) | (ul >> 3);
        public static void Rol(ref this ulong ul, int N) => ul = (ul << N) | (ul >> (64 - N));

    }

}
