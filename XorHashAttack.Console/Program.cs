using System.Diagnostics;
using TestLib.XorAttack;

const int ByteLength = 32;

var data = new byte[ByteLength];
Random.Shared.NextBytes(data);
var permitted = new byte[ByteLength * 12][];
for (int i = 0; i < permitted.Length; i++)
{
    var entry = new byte[ByteLength];
    Random.Shared.NextBytes(entry);
    permitted[i] = entry;
}

Console.WriteLine("Trying to break a XOR sum for a {0} bit hash...", ByteLength * 8);
var sw = Stopwatch.StartNew();

var result = XorAttackGenerator.BreakXor(data, permitted, OptimizeLevel.ToBaseHashes);
sw.Stop();
foreach (var item in result)
{
    Console.WriteLine(string.Concat(item.Select(m => m.ToString("X2"))));
}
Console.WriteLine("Hashes needed: {0}", result.Length);
Console.WriteLine("Computation took {0}", sw.Elapsed);

