namespace XorHashAttack.Lib.Internals
{
    /// <summary>
    /// Provider that creates and tracks <see cref="HashTree"/> nodes.
    /// It returns references to existing nodes if the hash is known,
    /// and tracks how often a node has been used
    /// </summary>
    /// <remarks>
    /// <b>This does not actually tracks usage outside of the instance.</b><br />
    /// This only works if all node additions to the hash tree are made through this instance.
    /// Node removal is not supported, but the entire instance can be cleared
    /// </remarks>
    internal class HashTreeProvider
    {
        /// <summary>
        /// All created nodes. Key is <see cref="HashTree.Hash"/>
        /// </summary>
        private readonly Dictionary<byte[], HashTree> _nodes = new(ByteArrayComparer.Instance);
        /// <summary>
        /// Usage count of nodes in <see cref="_nodes"/>. Same key
        /// </summary>
        private readonly Dictionary<byte[], int> _nodeCount = new(ByteArrayComparer.Instance);

        /// <summary>
        /// The first node ever created by this instance
        /// </summary>
        public HashTree RootNode { get; }

        /// <summary>
        /// Creates a new provider and creates the root node
        /// </summary>
        /// <param name="rootHash">Root node hash</param>
        public HashTreeProvider(byte[] rootHash)
        {
            RootNode = new HashTree(rootHash);
            _nodeCount.Add(RootNode.Hash, 1);
            _nodes[RootNode.Hash] = RootNode;
        }

        /// <summary>
        /// Resets this instance to the state right after the constructor has been called
        /// </summary>
        public void Reset()
        {
            _nodes.Clear();
            _nodeCount.Clear();
            RootNode.Sources.Clear();
        }

        /// <summary>
        /// Gets all created nodes that have been used an odd number of times
        /// </summary>
        /// <returns>Hash tree nodes</returns>
        public IEnumerable<HashTree> GetOddNodes()
        {
            foreach (var kv in _nodeCount.Where(m => m.Value % 2 != 0))
            {
                yield return _nodes[kv.Key];
            }
        }

        /// <summary>
        /// Returns a node for the given hash,
        /// creating a new one if neccessary,
        /// otherwise returning the existing node for the hash
        /// </summary>
        /// <param name="hash">Hash to get/add node</param>
        /// <param name="added">
        /// Whether the node was added (true), or an existing is reused (false)
        /// </param>
        /// <returns>Hash tree node</returns>
        public HashTree GetOrAdd(byte[] hash, out bool added)
        {
            if (_nodes.TryGetValue(hash, out var node))
            {
                added = false;
                ++_nodeCount[hash];
                return node;
            }
            added = true;
            _nodeCount[hash] = 1;
            return _nodes[hash] = new HashTree(hash);
        }
    }
}
