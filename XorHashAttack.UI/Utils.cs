using System.Diagnostics.CodeAnalysis;
using System.Security;

namespace XorHashAttack.UI
{
    internal static class Utils
    {
        public const int FileSizeLimit = 1024 * 1024 * 100;

        public static async Task<string[]> ReadLines(string fileName)
        {
            var lines = new List<string>();
            using var fs = File.OpenRead(fileName);
            if (fs.Length > FileSizeLimit)
            {
                throw new SecurityException($"File '{fileName}' is larger than {FileSizeLimit} bytes");
            }
            using var reader = new StreamReader(fs);
            while (!reader.EndOfStream)
            {
                var line = (await reader.ReadLineAsync())?.Trim();
                if (string.IsNullOrWhiteSpace(line) || line.IndexOfAny([';', '#']) == 0)
                {
                    continue;
                }
                if (!IsHexData(line))
                {
                    throw new IOException($"Invalid hash: '{line}'");
                }
                lines.Add(line.ToUpper());
            }
            return [.. lines];
        }

        public static bool IsHexData([AllowNull, NotNullWhen(true)] string data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return false;
            }
            return data.Length % 2 == 0 && data.All(char.IsAsciiHexDigit);
        }

        public static void EnsureValidHex([AllowNull, NotNull] string data)
        {
            if (!IsHexData(data))
            {
                throw new ArgumentException("Invalid hash", nameof(data));
            }
        }

        public static byte[] FromHex([AllowNull, NotNull] string text)
        {
            EnsureValidHex(text);

            return text
                .Chunk(2)
                .Select(m => byte.Parse(m, System.Globalization.NumberStyles.HexNumber))
                .ToArray();
        }

        public static string ToHex([AllowNull, NotNull] byte[] data)
        {
            ArgumentNullException.ThrowIfNull(data);
            return string.Concat(data.Select(m => m.ToString("X2")));
        }

        public static string TrimHash(string line)
        {
            const int limit = 16;
            const int slice = limit / 2;
            if (line.Length > limit)
            {
                return string.Concat(line.AsSpan(0, slice), "..", line.AsSpan(line.Length - slice));
            }
            return line;
        }
    }
}

