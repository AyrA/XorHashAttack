namespace XorHashAttack.Lib.Internals
{
    /// <summary>
    /// A XOR hash source
    /// </summary>
    internal class XorHashSource
    {
        /// <summary>
        /// Gets the first hash
        /// </summary>
        public byte[] H1 { get; }

        /// <summary>
        /// Gets the second hash
        /// </summary>
        public byte[] H2 { get; }

        /// <summary>
        /// Gets the result
        /// </summary>
        public byte[] Result { get; }

        /// <param name="h1">Hash 1</param>
        /// <param name="h2">Hash 2</param>
        /// <param name="result"><paramref name="h1"/> XOR <paramref name="h2"/></param>
        public XorHashSource(byte[] h1, byte[] h2, byte[] result)
        {
            ArgumentNullException.ThrowIfNull(h1);
            ArgumentNullException.ThrowIfNull(h2);
            ArgumentNullException.ThrowIfNull(result);

            Utils.ValidateXor(result, [h1, h2]);
            H1 = h1;
            H2 = h2;
            Result = result;
        }

        /// <param name="h1">Hash 1</param>
        /// <param name="h2">Hash 2</param>
        public XorHashSource(byte[] h1, byte[] h2)
        {
            ArgumentNullException.ThrowIfNull(h1);
            ArgumentNullException.ThrowIfNull(h2);

            H1 = h1;
            H2 = h2;
            Result = Utils.XOR(h1, h2);
        }

        public bool HasAny(byte[] hash)
        {
            ArgumentNullException.ThrowIfNull(hash);

            var i = ByteArrayComparer.Instance;
            return
                i.Equals(H1, hash) ||
                i.Equals(H2, hash) ||
                i.Equals(Result, hash);
        }
    }
}
