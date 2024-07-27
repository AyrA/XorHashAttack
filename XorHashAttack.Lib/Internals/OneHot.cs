namespace XorHashAttack.Lib.Internals
{
    /// <summary>
    /// Tracks a byte array with a "OneHot" flag.
    /// "One hot" is a byte array where exactly one bit is set
    /// </summary>
    internal class OneHot
    {
        /// <summary>
        /// OneHot data
        /// </summary>
        public bool[] Hot { get; }
        /// <summary>
        /// Hash
        /// </summary>
        public bool[] Data { get; }

        /// <summary>
        /// Creates a new instance from existing OneHot information and data
        /// </summary>
        /// <param name="hot">OneHot information</param>
        /// <param name="data">Hash</param>
        public OneHot(bool[] hot, bool[] data)
        {
            Hot = hot ?? throw new ArgumentNullException(nameof(hot));
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        /// <summary>
        /// Creates a new instance with the given bit index used as the OneHot bit
        /// </summary>
        /// <param name="index">OneHot bit</param>
        /// <param name="data">Hash</param>
        public OneHot(int index, bool[] data)
        {
            ArgumentNullException.ThrowIfNull(data);
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, data.Length);

            Hot = new bool[data.Length];
            Hot[index] = true;
            Data = data;
        }
    }
}
