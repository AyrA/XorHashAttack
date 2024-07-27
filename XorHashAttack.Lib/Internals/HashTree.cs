namespace XorHashAttack.Lib.Internals
{
    /// <summary>
    /// Represents a node in a hash tree
    /// </summary>
    /// <param name="hash">Base hash</param>
    internal class HashTree(byte[] hash)
    {
        /// <summary>
        /// Gets the base hash supplied in the constructor
        /// </summary>
        public byte[] Hash { get; } = hash;

        public string HashString => Utils.ToHex(Hash);

        /// <summary>
        /// Gets the list of source hashes that result in <see cref="Hash"/>
        /// when combined using XOR
        /// </summary>
        public List<HashTree> Sources { get; } = [];

        /// <summary>
        /// Adds a new hash to <see cref="Sources"/>
        /// </summary>
        /// <param name="node">Tree node</param>
        /// <returns><paramref name="node"/></returns>
        public HashTree AddSource(HashTree node)
        {
            Sources.Add(node);
            return node;
        }

        /// <summary>
        /// Recursively generates a mermaid entry for this node
        /// and all nodes in <see cref="Sources"/>
        /// </summary>
        /// <param name="lines">List to write mermaid lines to</param>
        /// <param name="outLine">
        /// Output line of parent entry to reference in this entry.
        /// This is null if this is the root node
        /// </param>
        public void GenerateMermaid(IList<string> lines, string? outLine = null)
        {
            if (Sources.Count > 0)
            {
                var itemLine = string.Format("{0}[{0}={1}]", HashString, string.Join("^", Sources.Select(m => m.HashString)));
                foreach (var source in Sources)
                {
                    if (outLine != null)
                    {
                        lines.Add($"{itemLine} --> {outLine}");
                    }
                    else
                    {
                        lines.Add($"{itemLine}");
                    }
                    source.GenerateMermaid(lines, HashString);
                }
            }
            else
            {
                lines.Add($"{HashString} --> {outLine}");
            }
        }
    }
}
