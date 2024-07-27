using System.Diagnostics;
using TestLib.XorAttack;
using XorHashAttack.Lib.Internals;

bool limitHashes = false;
bool verbose = false;
bool mermaid = false;
bool optimize = false;
string? hash = null;

if (args.Contains("/?"))
{
    Console.WriteLine(@"XorHashAttack.Console.exe <Hash> [/O|/M] [/V] [/L]
Hash: Hash to get to (in hexadecimal)
/O    Optimize output
/M    Generate mermaid JS diagram code
/V    Verbose output
/L    Limit ingested hash count

The hash list is read from standard input.
Output is the list of hashes that yield the supplied hash
when XOR summed.");
    return ErrorCodes.Help;
}

foreach (var arg in args)
{
    switch (arg.ToUpperInvariant())
    {
        case "/V":
            verbose = true;
            break;
        case "/L":
            limitHashes = true;
            break;
        case "/M":
            mermaid = true;
            break;
        case "/O":
            optimize = true;
            break;
        default:
            if (hash != null)
            {
                Console.Error.WriteLine("Hash already specified when processing argument '{0}'", arg);
                return ErrorCodes.ConflictingArgs;
            }
            hash = arg;
            break;
    }
}

if (mermaid && optimize)
{
    Console.Error.WriteLine("Cannot use /M and /O simultaneously");
    return ErrorCodes.ConflictingArgs;
}
if (string.IsNullOrWhiteSpace(hash))
{
    Console.Error.WriteLine("Invalid hash. Must be hexadecimal");
    return ErrorCodes.InvalidHash;
}
hash = string.Concat(hash.Where(m => !char.IsWhiteSpace(m)));

byte[] parsedHash;
try
{
    parsedHash = FromHex(hash);
}
catch
{
    Console.Error.WriteLine("Invalid hash. Must be hexadecimal");
    return ErrorCodes.InvalidHash;
}
List<byte[]> hashes = [];

while (!limitHashes || hashes.Count < parsedHash.Length * 16)
{
    var line = Console.ReadLine()?.Trim();
    if (line == null)
    {
        if (verbose)
        {
            Console.Error.WriteLine("List ended after {0} hashes", hashes.Count);
        }
        break;
    }
    if (string.IsNullOrWhiteSpace(line))
    {
        if (verbose)
        {
            Console.Error.WriteLine("Skipping empty line");
        }
    }
    else if (line.StartsWith(';') || line.StartsWith('#'))
    {
        if (verbose)
        {
            Console.Error.WriteLine("Skipping comment line");
        }
    }
    else
    {
        try
        {
            var parsed = FromHex(line);
            //If the hash exists in the list, we can just abort here and output the hash
            if (ByteArrayComparer.Instance.Equals(parsedHash, parsed))
            {
                if (verbose)
                {
                    Console.Error.WriteLine("Requested hash is contained in the hash list. Ending processing early");
                }
                Console.WriteLine(ToHex(parsed));
                return ErrorCodes.Success;
            }
            if (!hashes.Contains(parsed, ByteArrayComparer.Instance))
            {
                hashes.Add(parsed);
            }
            else if (verbose)
            {
                Console.Error.WriteLine("{0} is duplicate", ToHex(parsed));
            }
        }
        catch (Exception ex)
        {
            if (verbose)
            {
                Console.Error.WriteLine(ex.Message);
            }
            Console.Error.WriteLine("Invalid hash in list: {0}", line);
            return ErrorCodes.ListParseFail;
        }
    }
    if (verbose && limitHashes && hashes.Count >= parsedHash.Length * 16)
    {
        Console.Error.WriteLine("Reached maximum entry count. Will not ingest more hashes");
    }
}

if (hashes.Count < parsedHash.Length * 8)
{
    Console.Error.WriteLine("Too few hashes. Need at least {0} but read only {1}", parsedHash.Length * 8, hashes.Count);
    return ErrorCodes.TooFewHashes;
}

var sw = Stopwatch.StartNew();
if (verbose)
{
    Console.Error.WriteLine("Trying to reach '{0}' from a list of {1} hashes", hash, hashes.Count);
}
try
{
    if (mermaid)
    {
        XorAttackGenerator.RenderMermaid(parsedHash, [.. hashes], Console.Out);
    }
    else
    {
        var opt = optimize ? OptimizeLevel.ToBaseHashes : OptimizeLevel.None;
        var result = XorAttackGenerator.BreakXor(parsedHash, [.. hashes], opt);
        foreach (var item in result)
        {
            Console.WriteLine(ToHex(item));
        }
        if (verbose)
        {
            Console.Error.WriteLine("Used {0} hashes", result.Length);
        }
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine("Failed to compute XOR sum. {0}", ex.Message);
    return ErrorCodes.GeneratorFail;
}
sw.Stop();
if (verbose)
{
    Console.Error.WriteLine("Operation completed after {0}", sw.Elapsed);
}

return ErrorCodes.Success;

static byte[] FromHex(string hash)
{
    return hash.Chunk(2).Select(m => byte.Parse(m, System.Globalization.NumberStyles.HexNumber)).ToArray();
}

static string ToHex(IEnumerable<byte> data)
{
    return string.Concat(data.Select(m => m.ToString("X2")));
}

static class ErrorCodes
{
    public const int Success = 0;
    public const int ConflictingArgs = 1;
    public const int InvalidHash = 2;
    public const int ListParseFail = 3;
    public const int TooFewHashes = 4;
    public const int GeneratorFail = 5;
    public const int Help = 255;
}
