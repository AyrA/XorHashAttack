using System.Diagnostics;
using TestLib.XorAttack;
using XorHashAttack.Lib.Internals;

int result;
bool verbose = args.Contains("/V", StringComparer.OrdinalIgnoreCase);
var sw = Stopwatch.StartNew();

if (args.Contains("/?"))
{
    result = HandleHelpMode();
}
else if (args.Contains("/G", StringComparer.OrdinalIgnoreCase))
{
    result = HandleGenerateMode(args);
}
else
{
    result = HandleAttackMode(args);
}

sw.Stop();
if (verbose)
{
    Console.Error.WriteLine("Operation completed after {0} with exit code {1}", sw.Elapsed, result);
}

return result;

static int HandleAttackMode(string[] args)
{
    bool limitHashes = false;
    bool verbose = false;
    bool mermaid = false;
    bool optimize = false;
    string? hash = null;
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
    if (hash == null)
    {
        Console.Error.WriteLine("Hash argument not specified");
        return ErrorCodes.MissingArg;
    }
    hash = string.Concat(hash.Where(m => !char.IsWhiteSpace(m)));

    byte[] parsedHash;
    try
    {
        parsedHash = FromHex(hash);
    }
    catch (Exception ex)
    {
        if (verbose)
        {
            Console.Error.WriteLine(ex.Message);
        }
        Console.Error.WriteLine("Invalid hash. Must be hexadecimal");
        return ErrorCodes.InvalidHash;
    }
    if (verbose && parsedHash.All(m => m == 0))
    {
        Console.Error.WriteLine(@"Hash consists of nullbytes only.
This can be trivially reached by XORing any hash with itself.
Will try to find a solution using no hash multiple times anyways");
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
    return ErrorCodes.Success;
}

static int HandleGenerateMode(string[] args)
{
    int? bits = null;
    int? count = null;
    bool verbose = false;
    bool generator = false;

    foreach (var arg in args)
    {
        switch (arg.ToUpperInvariant())
        {
            case "/V":
                verbose = true;
                break;
            case "/G":
                generator = true;
                break;
            default:
                if (bits == null)
                {
                    if (int.TryParse(arg, out var tempBits))
                    {
                        if (tempBits <= 0)
                        {
                            Console.Error.WriteLine("Bit count must be at least 1");
                            return ErrorCodes.InvalidArg;
                        }
                        //Align to a byte boundary.
                        //If it isn't, the division will have decimals,
                        //which the ceiling function rounds up.
                        //"Ceil(x/n)*n" is a simple way to align x to an n boundary.
                        //"Floor" would do the same but rounding down instead of up
                        tempBits = (int)Math.Ceiling(tempBits / 8.0) * 8;
                        bits = tempBits;
                    }
                    else
                    {
                        Console.Error.WriteLine("Unable to parse '{0}' as number", arg);
                        return ErrorCodes.InvalidArg;
                    }
                }
                else if (count == null)
                {
                    if (int.TryParse(arg, out var tempCount))
                    {
                        if (tempCount <= 0)
                        {
                            Console.Error.WriteLine("Hash count must be at least 1");
                            return ErrorCodes.InvalidArg;
                        }
                        count = tempCount;
                    }
                    else
                    {
                        Console.Error.WriteLine("Unable to parse '{0}' as number", arg);
                        return ErrorCodes.InvalidArg;
                    }
                }
                else
                {
                    Console.Error.WriteLine("Unknown argument: '{0}'", arg);
                    return ErrorCodes.InvalidArg;
                }
                break;
        }
    }
    if (!generator)
    {
        throw new InvalidOperationException("Generator mode without /G");
    }
    if (bits == null)
    {
        Console.Error.WriteLine("Missing bits argument");
        return ErrorCodes.MissingArg;
    }
    count ??= bits.Value * 2;

    if (verbose)
    {
        Console.Error.WriteLine("Generating {0} hashes", count);
    }
    var hash = new byte[bits.Value / 8];
    for (var i = 0; i < count; i++)
    {
        Random.Shared.NextBytes(hash);
        Console.WriteLine(ToHex(hash));
    }

    return ErrorCodes.Success;
}

static int HandleHelpMode()
{
    Console.WriteLine(@"XorHashAttack.Console.exe <Hash> [/O|/M] [/V] [/L]
Performing XOR sum attack:

    Hash:  Hash to get to (in hexadecimal)
    /O     Optimize output
    /M     Generate mermaid JS diagram code
    /V     Verbose output
    /L     Limit ingested hash count

    The hash list is read from standard input.
    Output is the list of hashes that yield the supplied hash
    when XOR summed.

XorHashAttack.Console.exe /G <Bits> [Count] [/V]
Generating attack list:

    /G     Generate hashes
    /V     Verbose output
    Bits:  Number of bits in each hash
           This will be rounted up to the nearest byte
    Count: Number of hashes to generate.
           If not specified, generates bits*2 hashes

    The list is written to standard output");
    return ErrorCodes.Help;
}

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
    public const int InvalidArg = 6;
    public const int MissingArg = 7;
    public const int Help = 255;
}
