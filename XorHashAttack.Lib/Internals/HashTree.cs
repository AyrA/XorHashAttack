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
        /// <param name="output">
        /// Output to write to
        /// </param>
        /// <param name="cancellationToken">Token to cancel the operation</param>
        public void GenerateMermaid(TextWriter output, CancellationToken? cancellationToken)
            => GenerateMermaid([], output, null, cancellationToken);

        /// <summary>
        /// Recursively generates a mermaid entry for this node
        /// and all nodes in <see cref="Sources"/>
        /// </summary>
        /// <param name="processedNodes">
        /// List of nodes that have been processed already. Empty for the root node
        /// </param>
        /// <param name="output">Output to write to</param>
        /// <param name="outLine">
        /// Output line of parent entry to reference in this entry.
        /// This is null if this is the root node
        /// </param>
        /// <param name="cancellationToken">Token to cancel the operation</param>
        private void GenerateMermaid(IList<HashTree> processedNodes, TextWriter output, string? outLine, CancellationToken? cancellationToken)
        {
            cancellationToken?.ThrowIfCancellationRequested();
            if (processedNodes.Contains(this))
            {
                return;
            }
            processedNodes.Add(this);
            if (Sources.Count > 0)
            {
                var itemLine = string.Format("{0}[{0}={1}]", HashString, string.Join("^", Sources.Select(m => m.HashString)));
                string lineString = outLine != null ? $"{itemLine} --> {outLine}" : itemLine;
                output.WriteLine(lineString);
                foreach (var source in Sources)
                {
                    source.GenerateMermaid(processedNodes, output, HashString, cancellationToken);
                }
            }
            else
            {
                output.WriteLine($"{HashString} --> {outLine}");
            }
            //Root node has this set to null
            if (outLine == null)
            {
                output.WriteLine("%% Total lines: {0}", processedNodes.Count);
            }
        }
    }
}
