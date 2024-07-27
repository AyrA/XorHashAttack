using System.Diagnostics;
using XorHashAttack.Lib.Internals;

namespace TestLib.XorAttack
{
    /// <summary>
    /// Provides means to attack a XOR sum
    /// </summary>
    public static class XorAttackGenerator
    {
        /// <summary>
        /// Combines all values in <paramref name="provided"/> using XOR
        /// and ensures that they result in <paramref name="requested"/>
        /// </summary>
        /// <param name="requested">Requested value</param>
        /// <param name="provided">Provided value</param>
        public static void ValidateXor(byte[] requested, IEnumerable<byte[]> provided)
            => Utils.ValidateXor(requested, provided);

        /// <summary>
        /// Combines hashes from <paramref name="given"/> using XOR
        /// to reach <paramref name="requested"/>
        /// </summary>
        /// 
        /// <param name="requested">The byte data to create using just XOR</param>
        /// <param name="given">
        /// The permitted starting values.
        /// Each entry must have the same length as <paramref name="requested"/>,
        /// and the list should contain as many entries as possible to increase the chance of success.
        /// Ideally it contains more entries than <paramref name="requested"/>.Length*8
        /// </param>
        /// <param name="level">
        /// Optimization level to apply<br />
        /// • <see cref="OptimizeLevel.None"/>: Returned value contains intermediate hashes
        /// that were dynamically generated using XOR from the given hash list.<br />
        /// • <see cref="OptimizeLevel.ToBaseHashes"/>: Intermeidate hashes are resolved back
        /// to hashes from the given hash list. The returned value only contains hashes from
        /// <paramref name="given"/>. Runtime is approximately doubled, and memory consumption
        /// may rise significantly.
        /// </param>
        /// <param name="cancellationToken">Optional token to cause early function abort</param>
        /// 
        /// <returns>
        /// Hashes that when combined using XOR
        /// will yield <paramref name="requested"/>
        /// </returns>
        /// 
        /// <remarks>
        /// • This only works reliably if <paramref name="given"/>
        /// is sufficiently large and diverse, ideally it has random distribution of bits.
        /// Rule of thumb is that it must contain at least as many entries
        /// as <paramref name="requested"/> contains bits (Length * 8).<br />
        /// • If <paramref name="requested"/> cannot be obtained from <paramref name="given"/>,
        /// an exception is thrown.
        /// </remarks>
        public static byte[][] BreakXor(byte[] requested, byte[][] given, OptimizeLevel level, CancellationToken? cancellationToken = null)
        {
            SanityChecks(requested, given);
            if (!Enum.IsDefined(level))
            {
                throw new ArgumentOutOfRangeException(nameof(level));
            }

            //Find set of hashes that generate "requested"
            var xorsumResult = FindHashes(requested, given, cancellationToken);
            Debug.Print("Reached  {0}", Utils.ToHex(requested));
            Debug.Print("Required {0} hashes", xorsumResult.ComputedHashes.Length);

            if (level == OptimizeLevel.None)
            {
                return xorsumResult.ComputedHashes;
            }

            //In theory we can return "xorsumResult.ComputedHashes" here,
            //but it contains hashes not directly in "given" but created using XOR of hashes.
            //We build the entire hash tree, then we can strip all but the base hashes from it.
            var htp = CreateHashTree(requested, xorsumResult, cancellationToken);

            //We add all hashes from the tree list that are in "allowed" into the return value.
            //We only do this with those that appear an odd number of times,
            //because an even number of occurences cancels the value in a xorsum
            return GetBaseHashes(htp, xorsumResult, cancellationToken);
        }

        /// <summary>
        /// Combines hashes from <paramref name="given"/> using XOR
        /// to reach <paramref name="requested"/>,
        /// writing the resulting hash XOR tree to <paramref name="output"/>
        /// in a mermaid JS compatible format.
        /// </summary>
        /// <param name="requested">Requested hash</param>
        /// <param name="given">Given hash</param>
        /// <param name="output">Mermaid output</param>
        /// <remarks>
        /// Mermaid has an entity limit of 500,
        /// and anything above 16 bits is likely going to exceed this limit.
        /// If you're going to use this for large hashes,
        /// be prepared to alter the "maxEdges" parameter of the mermaid initialization.
        /// </remarks>
        public static void RenderMermaid(byte[] requested, byte[][] given, TextWriter output, CancellationToken? cancellationToken = null)
        {
            SanityChecks(requested, given);
            ArgumentNullException.ThrowIfNull(output);

            var htp = CreateHashTree(requested, FindHashes(requested, given));

            //The mermaid code is based on the unoptimized hash tree.
            //This way we can see how the tree looks that the hash finder created
            output.WriteLine("flowchart TD");
            var lines = new List<string>();
            htp.RootNode.GenerateMermaid(lines, null);
            lines.ForEach(Console.WriteLine);
        }

        /// <summary>Checks parameters for conformity</summary>
        /// <param name="requested">Requested hash</param>
        /// <param name="given">Permitted hashes to build XOR sum</param>
        /// <remarks>
        /// • Neither argument may be null<br />
        /// • <paramref name="requested"/> cannot be an empty array<br />
        /// • Length of all given hashes must match <paramref name="requested"/><br />
        /// • <paramref name="given"/> must contain at least as many hashes as <paramref name="given"/> has bits.
        /// </remarks>
        private static void SanityChecks(byte[] requested, byte[][] given)
        {
            ArgumentNullException.ThrowIfNull(requested);
            ArgumentNullException.ThrowIfNull(given);
            if (requested.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(requested), "Requested hash has zero length");
            }
            if (given.Any(m => m == null || m.Length != requested.Length))
            {
                throw new ArgumentException("At least one given hash does not match the length of the requested hash", nameof(given));
            }
            if (given.Length < requested.Length * 8)
            {
                throw new ArgumentException($"Too few given values. Need at least {requested.Length * 8}", nameof(requested));
            }
        }

        /// <summary>
        /// Resolves all intermeidate nodes of the hash tree into base hashes
        /// from <paramref name="xorsumResult"/>
        /// </summary>
        /// <param name="htp">
        /// Populated hash tree from <see cref="CreateHashTree(byte[], XorsumResult)"/>
        /// </param>
        /// <param name="xorsumResult">
        /// XOR sum result from <see cref="FindHashes(byte[], byte[][])"/>
        /// </param>
        /// <param name="cancellationToken">Token to cause early abort</param>
        /// <returns>
        /// Set of values from <see cref="XorsumResult.PermittedHashes"/>
        /// that generates <see cref="HashTreeProvider.RootNode"/>
        /// </returns>
        private static byte[][] GetBaseHashes(HashTreeProvider htp, XorsumResult xorsumResult, CancellationToken? cancellationToken = null)
        {
            var reduced = new List<byte[]>();
            var allowed = xorsumResult.PermittedHashes
                .Where(m => !m.All(n => n == 0))
                .ToArray();
            foreach (var node in htp.GetOddNodes())
            {
                cancellationToken?.ThrowIfCancellationRequested();
                if (allowed.Contains(node.Hash, ByteArrayComparer.Instance))
                {
                    reduced.Add(node.Hash);
                }
            }
            Debug.Print("Reduced: {0} hashes", reduced.Count);

            return [.. reduced.OrderBy(Utils.ToHex)];
        }

        /// <summary>
        /// Converts the result of <see cref="FindHashes(byte[], byte[][])"/> into a hash tree
        /// </summary>
        /// <param name="requested">Requested hash</param>
        /// <param name="xorsumResult">Result from <see cref="FindHashes(byte[], byte[][])"/></param>
        /// <param name="cancellationToken">Token to cause early abort</param>
        /// <returns>Populated hash tree provider</returns>
        private static HashTreeProvider CreateHashTree(byte[] requested, XorsumResult xorsumResult, CancellationToken? cancellationToken = null)
        {
            var allowed = xorsumResult.PermittedHashes
                .Where(m => !m.All(n => n == 0)) //Strip hashes made up entirely of nullbytes
                .ToArray();

            Stack<HashTree> stack = [];
            HashTreeProvider htp = new(requested);

            foreach (var h in xorsumResult.ComputedHashes)
            {
                cancellationToken?.ThrowIfCancellationRequested();
                var source = htp.RootNode.AddSource(htp.GetOrAdd(h, out _));
                stack.Push(source);
            }

            //Find the source of every used hash
            while (stack.Count > 0)
            {
                cancellationToken?.ThrowIfCancellationRequested();
                var current = stack.Pop();
                //A zero hash should not exist here
                if (current.Hash.All(m => m == 0))
                {
                    throw new InvalidDataException("Implementation error. Encountered a null hash");
                }
                //If not in allowed list, it must be somewhere in "generatedHashes"
                if (!allowed.Contains(current.Hash, ByteArrayComparer.Instance))
                {
                    if (!xorsumResult.GeneratedHashes.TryGetValue(current.Hash, out var generated))
                    {
                        throw new InvalidDataException($"Hash {current.Hash} has no source");
                    }
                    HashTree node1 = current.AddSource(htp.GetOrAdd(generated.H1, out bool added1));
                    HashTree node2 = current.AddSource(htp.GetOrAdd(generated.H2, out bool added2));
                    //We always add the hashes, but skip recursively going into the generated nodes,
                    //if they're not new instances
                    if (added1)
                    {
                        stack.Push(node1);
                    }
                    if (added2)
                    {
                        stack.Push(node2);
                    }
                }
            }

            return htp;
        }

        /// <summary>
        /// Computes a set of hashes that when XOR summed together will yield <paramref name="requested"/>.
        /// All hashes are either directly contained in <paramref name="permitted"/>,
        /// or are the result of XOR summing two or more values of <paramref name="permitted"/>
        /// </summary>
        /// <param name="requested">Requested hash</param>
        /// <param name="permitted">Permitted hash values</param>
        /// <param name="cancellationToken">Token to cause early abort</param>
        /// <returns>XOR sum result</returns>
        private static XorsumResult FindHashes(byte[] requested, byte[][] permitted, CancellationToken? cancellationToken = null)
        {
            //For hashes created by conbining two others using XOR.
            //This is only relevant if a mermaid chart is to be generated.
            Dictionary<byte[], XorHashSource> generatedHashes = new(ByteArrayComparer.Instance);
            //The final list of hashes required to XOR into the requested value
            List<byte[]> ret = [];

            //Convert into boolean arrays
            //This is strictly not necessary, and in fact quite wasteful,
            //but it makes bit selection much easier
            var requestedValue = Utils.ToBoolArray(requested);
            var permittedSourceValues = permitted.Select(Utils.ToBoolArray).ToArray();
            var oneHotHashes = permittedSourceValues.Select((m, i) => new OneHot(i % m.Length, m)).ToArray();
            var ans = new bool[requestedValue.Length];

            for (var i = 0; i < requestedValue.Length; i++)
            {
                cancellationToken?.ThrowIfCancellationRequested();
                OneHot? chosen = null;
                foreach (var b in oneHotHashes)
                {
                    cancellationToken?.ThrowIfCancellationRequested();
                    if (b.Data[i] == requestedValue[i])
                    {
                        Utils.XOR(ans, ans, b.Data);
                        Utils.XOR(requestedValue, requestedValue, b.Data);
                        ret.Add(Utils.ToByteArray(b.Data));
                    }
                }
                foreach (var b in oneHotHashes)
                {
                    cancellationToken?.ThrowIfCancellationRequested();
                    if (b.Data[i])
                    {
                        chosen = b;
                        break;
                    }
                }

                if (chosen == null)
                {
                    throw new InvalidDataException("No hash could be chosen. The list of permitted inputs is likely not random enough.");
                }

                List<OneHot> newOneHotHashes = [];
                foreach (var b in oneHotHashes)
                {
                    cancellationToken?.ThrowIfCancellationRequested();
                    if (!b.Data[i])
                    {
                        newOneHotHashes.Add(b);
                    }
                    else
                    {
                        if (!Utils.Compare(b.Data, chosen.Data))
                        {
                            var newHash = Utils.XOR(b.Data, chosen.Data);
                            generatedHashes[Utils.ToByteArray(newHash)] = new XorHashSource(Utils.ToByteArray(b.Data), Utils.ToByteArray(chosen.Data), Utils.ToByteArray(newHash));
                            newOneHotHashes.Add(new(Utils.XOR(b.Hot, chosen.Hot), newHash));
                        }
                    }
                }
                oneHotHashes = [.. newOneHotHashes];
            }
            //The result should be the requested value
            if (!Utils.Compare(Utils.ToByteArray(ans), requested))
            {
                throw new Exception("Implementation error");
            }
            //Since we're XORing data, we can remove hashes that exist an even number of times
            ret = Utils.OnlyOddOnes(ret);
            //Assert that XORing all values will actually result in "requested"
            Utils.ValidateXor(requested, ret);
            return new([.. ret], generatedHashes, [.. permitted]);
        }
    }
}
