# XOR Hash Attack Demo

This application demonstrates a practical attack against hashes that were combined using XOR operation.

The library can generate a XOR sum based on a supplied list of hashes to reach a desired target hash as long as the list of provided hashes is long and diverse enough. The minimum length is the number of bits in the hash algorithm, but usually a few extra hashes are needed. Rule of thumb is that the list should contain at least 1.5 times as many hashes as the algorithm contains bits. For a 256 bit hash the list should therefore contain 384 hashes.

The hash algorithm itself is irrelevant, only the binary hash itself that is to be reached must be known.

## Paper

See *A Generalized Birthday Problem.pdf* for a mathematical explanation for how this works.

## Attack Scenario

This can be used whenever a XOR combined hash sum must be broken,
and a large number of arbitrary hashes are permitted.

The large hash count is key. To break a 128 bit XOR sum you will need on average around 170 hashes. If the protocol/system/implementation you try to break does not permits this, there is an extremely high chance you will not be able to pull this off.

### Example: File checksum utility

A file checksum utility may calculate the SHA256 of all files and XOR combine the hashes into a single one to serve as a condensed checksum of all files.

A second set of files can trivially be created to yield the same hash:

1. Generate a sufficiently large number of files in a temporary directory (at least bitcount-of-hash&times;1.5, but &times;2 is better)
2. Add the hash of each file to a list
3. Use the XorHashAttack tool with `/O` to get the subset of hashes needed from the list to get to the same hash as the original files
4. Copy the files whose hash appears in the output to the destination directory
5. The destination directory will now yield the same checksum as the original directory, even though the set of files is completely different.

We successfully created a file listing that matches in checksum.

## Performance

The code is written to be easy to understand, and not to be fast or efficient.
Running it for hash sizes above 512 bits is not recommended.
On a modern and powerful machine it runs about 10-20 seconds for 512 bits,
and 1-2 seconds for 256 bits.

## Usage

This project provides a UI, command line utility, and library.

To use it in your projects, just copy the library to your solution and add it as reference, or build the library and just add the DLL.

The UI is a simple "double click and go" type of application. Simply fill out the hash fields and click the button to break the XOR sum. You don't have to copy and paste the hash list, but instead you can drag one or more (at once) files onto the textbox to read hashes from them. The expected structure of the files is explained in the "Source Hashes" command line help section further below.

Note: If you build it in debug mode it autofills randomly generated values for testing.

You can also use it using the provided command line tool.
Usage is as follows:

```
; finding XOR sum solution
XorHashAttack.Console.exe <Hash> [/O|/M] [/V] [/L] < hashes.txt
OtherCommand | XorHashAttack.Console.exe <Hash> [/O|/M] [/V] [/L]

; creating a hash list
XorHashAttack.Console.exe /G <Bits> <Count> [/V]
```

### `Hash` Requested hash

This is the hash the application is tasked to reach using the provided hash list. The hash must be supplied as hexadecimal without `0x` prefix.
It may be uppercase or lowercase.

### Source hashes

The permitted source hashes are piped in from either a text file (first example), or another command (second example).
The supplied hash list must contain one hash in hexadecimal format per line. All whitespace is stripped from the line. Empty lines are permitted and silently skipped. Lines starting in `;` or `#` (after optional whitespace) are discarded. Other lines will cause the command to abort with an error. Duplicates are skipped.

The absolute minimum number of hashes is the number of bits in the requested hash. If the requested hash is `ABCD`, at least 16 hashes must be present in the list.
If the list is too short, the tool will abort with an error.

### Output

The output is sent to the console, one hash per line in uppercase hexadecimal. It may be piped into another command for further processing.

### `/L` Limit hash count

By default the entire hash list is read, but if the list is much longer than necessary, this argument may be supplied to only read twice as many hashes as the requested hash has bits. A hash of `ABCD` would be 16 bits, therefore `/L` would stop reading after 32 hashes.

### `/O` Optimize output

By default, the generated hash list contains hashes that are not actually contained in the provided hash list, but are a result of combining various hashes from the list using XOR.
By specifying this argument, the generated hashes are untangled into the base hashes, ensuring that the output only contains hashes from the provided list. The output will be much shorter this way.
Optimization for long hashes (512 bits or more) may take a few seconds, but is usually recommended.

### `/M` Mermaid JS output

This generates mermaid JS flowchart text of the entire hash tree. For hash sizes above 16 bits (2 bytes) this likely contains more entries
than mermaid is willing to process by default, and you may need to raise the limit.

The argument is mutually exclusive with `/O` because the mermaid output always renders the unoptimized hash tree. The optimized tree would just be a bunch of hashes directly pointing to the end result.

**CAUTION!** This is an incredibly verbose process. Generating the mermaid output for a 512 bit hash XOR sum easily creates 100 MB of text because it will contain around 200'000 lines. Creating this (including the hash computations) would take around 1-2 minutes. Mermaid output is for demonstration purposes only and should not be used for hashes longer than 16 bits (2 bytes) because the mermaid tool would likely hang for a long time to parse and render the diagram.

### `/V` Verbose output

This writes extra output to the console. The data is written to the error stream to not affect the hash list output. It's not too I/O intensive and should not increase runtime when used.

### `/G` Generate list

This argument can be used to generate a random list of hashes. It is not not needed for breaking hashes but may simply be used as a convenience feature for testing. The output can be piped into itself for simple testing purposes:

```
XorHashAttack.Console /G 16 | XorHashAttack.Console ABCD /O
```

### `Bits` Hash size

The number of bits in the generated hashes.
This is rounded up to the next byte boundary because this application internally works on bytes, not bits.
This argument is required.

### `Count` Hash count

Number of hashes to generate.
If not specified, generates two times as many hashes as the bits argument.
If bits is 16, 32 hashes will be generated.
If specified, this must be after the bits argument.

### `/?` Help

Shows simple command line help
