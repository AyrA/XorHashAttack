namespace XorHashAttack.Lib.Internals
{
    /// <summary>
    /// Result for a xorsum operation
    /// </summary>
    /// <param name="ComputedHashes">Hashes that when XOR summed will yield the desired target hash</param>
    /// <param name="GeneratedHashes">
    /// Hashes that were generated from <paramref name="PermittedHashes"/> as part of the sum process
    /// </param>
    /// <param name="PermittedHashes">Hashes permitted to be used in the XOR sum</param>
    internal record XorsumResult(byte[][] ComputedHashes, Dictionary<byte[], XorHashSource> GeneratedHashes, byte[][] PermittedHashes);
}
