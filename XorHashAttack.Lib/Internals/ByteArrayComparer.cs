using System.Diagnostics.CodeAnalysis;

namespace XorHashAttack.Lib.Internals
{
    /// <summary>
    /// Compares byte arrays by value instead of by reference
    /// </summary>
    internal class ByteArrayComparer : IEqualityComparer<byte[]>
    {
        /// <summary>
        /// The sole instance
        /// </summary>
        public static readonly ByteArrayComparer Instance = new();

        /// <summary>
        /// Private constructor to prevent manual construction
        /// </summary>
        private ByteArrayComparer() { }

        /// <summary>
        /// Checks if the two byte arrays are equal in value.
        /// </summary>
        /// <param name="x">Value 1</param>
        /// <param name="y">Value 2</param>
        /// <returns>true if equal</returns>
        /// <remarks>Internally uses <see cref="Utils.Compare(byte[]?, byte[]?)"/></remarks>
        public bool Equals(byte[]? x, byte[]? y)
        {
            return Utils.Compare(x, y);
        }

        /// <summary>
        /// Gets a custom hash code for each array based on the value rather than the reference
        /// </summary>
        /// <param name="obj">Byte array</param>
        /// <returns>Hash code</returns>
        /// <remarks>
        /// For performance reasons, this will at most read the first 256 entries
        /// </remarks>
        public int GetHashCode([DisallowNull] byte[] obj)
        {
            ArgumentNullException.ThrowIfNull(obj);

            var hashCode = BitConverter.GetBytes(Instance.GetHashCode());
            for (var i = 0; i < Math.Min(256, obj.Length); i++)
            {
                hashCode[i % 4] ^= obj[i];
            }
            return BitConverter.ToInt32(hashCode);
        }
    }
}
