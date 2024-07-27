namespace TestLib.XorAttack
{
    /// <summary>
    /// XOR sum builder optimiziation level
    /// </summary>
    public enum OptimizeLevel
    {
        /// <summary>
        /// No optimization.
        /// This is the fastest option
        /// </summary>
        None,
        /// <summary>
        /// Reduce all intermediate hashes
        /// (hashes resulting from XOR of two other hashes, posibly recursively)
        /// to base hashes from the original list of given hashes.
        /// </summary>
        ToBaseHashes
    }
}
