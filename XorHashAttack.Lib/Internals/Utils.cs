using System.Runtime.InteropServices;

namespace XorHashAttack.Lib.Internals
{
    /// <summary>
    /// Utility functions
    /// </summary>
    internal static class Utils
    {
        /// <summary>
        /// Fastest way to compare byte arrays in .NET
        /// </summary>
        /// <param name="a">Data 1</param>
        /// <param name="b">Data 2</param>
        /// <param name="count">
        /// Number of bytes to compare. Usually <tt>Math.Min(a.Length,b.Length)</tt></param>
        /// <returns>amount of difference in first byte that differs. Zero if equal</returns>
        [DllImport("msvcrt.dll", EntryPoint = "memcmp")]
        private static extern int CompareBytes([In] byte[] a, [In] byte[] b, int count);

        /// <summary>
        /// Cursed version of <see cref="CompareBytes(byte[], byte[], int)"/>.
        /// </summary>
        /// <param name="a">Data 1</param>
        /// <param name="b">Data 2</param>
        /// <param name="count">
        /// Number of bytes to compare.
        /// Usually <tt>Math.Min(a.Length,b.Length)*sizeof(int)</tt>
        /// </param>
        /// <returns>amount of difference in first byte that differs. Zero if equal</returns>
        /// <remarks>
        /// The native memcmp function takes two pointers and an integer as arguments.
        /// It compares as many "memory cells" (bytes on modern systems)
        /// as <paramref name="count"/> specifies.
        /// Because of this, any array type can be supplied as argument,
        /// as long as you know how much memory an array entry occupies.
        /// </remarks>
        [DllImport("msvcrt.dll", EntryPoint = "memcmp")]
        private static extern int CompareBools([In] bool[] a, [In] bool[] b, int count);

        /// <summary>
        /// Compares two arrays
        /// </summary>
        /// <param name="a">Array 1</param>
        /// <param name="b">Array 2</param>
        /// <returns>true if identical</returns>
        /// <remarks>
        /// Also considers the arrays identical if they're both null
        /// </remarks>
        public static bool Compare(bool[]? a, bool[]? b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }
            if (a == null || b == null)
            {
                return false;
            }
            if (a.Length != b.Length)
            {
                return false;
            }
            return CompareBools(a, b, a.Length * sizeof(int)) == 0;
        }

        /// <inheritdoc cref="Compare(bool[], bool[])"/>
        public static bool Compare(byte[]? a, byte[]? b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }
            if (a == null || b == null)
            {
                return false;
            }
            if (a.Length != b.Length)
            {
                return false;
            }

            return CompareBytes(a, b, a.Length) == 0;
        }

        /// <summary>
        /// Ensures that the XOR sum of <paramref name="provided"/>
        /// results in <paramref name="requested"/>.
        /// </summary>
        /// <param name="requested">Requested hash value</param>
        /// <param name="provided">Provided hash list</param>
        /// <exception cref="InvalidDataException">Thrown if XOR sum is invalid</exception>
        public static void ValidateXor(byte[] requested, IEnumerable<byte[]> provided)
        {
            ArgumentNullException.ThrowIfNull(requested);
            ArgumentNullException.ThrowIfNull(provided);

            byte[] sum = new byte[requested.Length];
            foreach (var hash in provided)
            {
                XOR(sum, sum, hash);
            }
            if (!Compare(sum, requested))
            {
                throw new InvalidDataException("Provided hash list does not XOR sum to requested value");
            }
        }

        /// <summary>
        /// Turns a hex string into a byte array
        /// </summary>
        /// <param name="s">Hex string</param>
        /// <returns>byte array</returns>
        public static byte[] FromHex(string s)
        {
            return s
                .Chunk(2)
                .Select(m => byte.Parse(m, System.Globalization.NumberStyles.AllowHexSpecifier))
                .ToArray();
        }

        /// <summary>
        /// Returns entries that occur an odd number of times in data
        /// </summary>
        /// <param name="data">Data to filter for odd number of identical entries</param>
        /// <returns>Entries occuring an odd number of times</returns>
        public static List<byte[]> OnlyOddOnes(IEnumerable<byte[]> data)
        {
            if (data is null)
            {
                return [];
            }

            Dictionary<byte[], int> counts = new(ByteArrayComparer.Instance);
            foreach (var b in data)
            {
                if (!counts.TryAdd(b, 1))
                {
                    ++counts[b];
                }
            }
            return counts
                .Where(m => m.Value % 2 != 0)
                .Select(m => m.Key)
                .ToList();
        }

        /// <summary>
        /// Converts the byte data into a hex string
        /// </summary>
        /// <param name="data">Byte data</param>
        /// <returns>Hex string</returns>
        public static string ToHex(IEnumerable<byte> data)
        {
            ArgumentNullException.ThrowIfNull(data);
            return string.Concat(data.Select(m => m.ToString("X2")));
        }

        /// <summary>
        /// Converts the giben boolean array into a hex string,
        /// treating each bool entry as a bit
        /// </summary>
        /// <param name="data">Bool data</param>
        /// <returns>Hex string</returns>
        public static string ToHex(bool[] data)
        {
            ArgumentNullException.ThrowIfNull(data);
            return string.Concat(ToByteArray(data).Select(m => m.ToString("X2")));
        }

        /// <summary>
        /// Converts the boolean array into a byte array
        /// by packing 8 bools into a byte
        /// </summary>
        /// <param name="data">Boolean data</param>
        /// <returns>Byte data</returns>
        /// <remarks>
        /// Last byte is padded on the left with zeros
        /// if <paramref name="data"/> length is not a multiple of 8
        /// </remarks>
        public static byte[] ToByteArray(bool[] data)
        {
            ArgumentNullException.ThrowIfNull(data);
            return [.. data.Chunk(8).Select(ToByte)];
        }

        /// <summary>
        /// Converts boolean data into a single byte
        /// </summary>
        /// <param name="data">Boolean data</param>
        /// <returns>Byte</returns>
        /// <remarks>
        /// Returned value is padded on the left with bits set to zero if necessary.
        /// If data has more than 8 entries,
        /// overflow occurs and the first booleans will be cut off
        /// </remarks>
        public static byte ToByte(bool[] data)
        {
            ArgumentNullException.ThrowIfNull(data);
            byte ret = 0;
            foreach (var b in data)
            {
                ret <<= 1;
                if (b)
                {
                    ret |= 1;
                }
            }
            return ret;
        }

        /// <summary>
        /// XOR combines two arrays and returns the result
        /// </summary>
        /// <param name="a">Array 1</param>
        /// <param name="b">Array 2</param>
        /// <returns><paramref name="a"/> XOR <paramref name="b"/></returns>
        public static byte[] XOR(byte[] a, byte[] b)
        {
            var result = new byte[a.Length];
            XOR(result, a, b);
            return result;
        }

        /// <inheritdoc cref="XOR(byte[], byte[])"/>
        public static bool[] XOR(bool[] a, bool[] b)
        {
            var result = new bool[a.Length];
            XOR(result, a, b);
            return result;
        }

        /// <summary>
        /// XOR combines two arrays and stores the result in <paramref name="result"/>
        /// </summary>
        /// <param name="a">Array 1</param>
        /// <param name="b">Array 2</param>
        /// <param name="result"><paramref name="a"/> XOR <paramref name="b"/></param>
        /// <remarks>
        /// <paramref name="result"/> can be identical to <paramref name="a"/> to cause <tt>a^=b</tt>,
        /// or identical to <paramref name="b"/> to cause <tt>b^=a</tt>
        /// </remarks>
        public static void XOR(bool[] result, bool[] a, bool[] b)
        {
            for (int i = 0; i < Math.Min(a.Length, b.Length); i++)
            {
                result[i] = a[i] ^ b[i];
            }
        }

        /// <inheritdoc cref="XOR(bool[], bool[], bool[])"/>
        public static void XOR(byte[] result, byte[] a, byte[] b)
        {
            for (int i = 0; i < Math.Min(a.Length, b.Length); i++)
            {
                result[i] = (byte)(a[i] ^ b[i]);
            }
        }

        /// <summary>
        /// Converts a byte array into a boolean array,
        /// resulting in 8 boolean entries per byte entry
        /// </summary>
        /// <param name="data">Byte array</param>
        /// <returns>Boolean array</returns>
        public static bool[] ToBoolArray(byte[] data)
        {
            ArgumentNullException.ThrowIfNull(data);
            return data.SelectMany(ToBoolData).ToArray();
        }

        /// <summary>
        /// Converts a single byte into an 8 entry bool array
        /// </summary>
        /// <param name="data">Byte</param>
        /// <returns>Boolean array</returns>
        public static bool[] ToBoolData(byte data)
        {
            ArgumentNullException.ThrowIfNull(data);
            return [
                0 != (data & 0b10000000),
                0 != (data & 0b01000000),
                0 != (data & 0b00100000),
                0 != (data & 0b00010000),
                0 != (data & 0b00001000),
                0 != (data & 0b00000100),
                0 != (data & 0b00000010),
                0 != (data & 0b00000001)
            ];
        }
    }
}
