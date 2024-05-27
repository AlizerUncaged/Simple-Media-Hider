using System.Drawing;
using System.Text.Json;
using Pastel;

class Program
{
    private const string DefaultPaths = @"C:\Users\Gumayusi\Desktop\carn.txt";
    private const string ExtensionsFile = @"extensions.json";

    /// <summary>
    /// Simple media file hider that hides media files by scrambling the extension,
    /// just enough to hide from galleries and media scanners.
    /// </summary>
    static void Main()
    {
        new Program().StartAsync().GetAwaiter().GetResult();
    }

    public async Task StartAsync()
    {
        Console.Write("Enter file of paths: ");

        string fileContainingPath = Console.ReadLine()?.Replace("\"", "");

        if (string.IsNullOrWhiteSpace(fileContainingPath) && File.Exists(DefaultPaths))
        {
            Console.WriteLine($"{"Defaulting to".Pastel(Color.Orange)} {DefaultPaths}");

            fileContainingPath = DefaultPaths;
        }

        if (!File.Exists(fileContainingPath))
        {
            Console.WriteLine($"{"File does not exist!".Pastel(Color.Orange)} Continuing...");
        }

        Console.Write("Encrypt or Decrypt? (e/d): ");

        var encryptOrDecryptChoice = Console.ReadLine();

        var fileCount = 0;

        var pathsInFileContainingFilePath = await File.ReadAllLinesAsync(fileContainingPath);

        IEnumerable<string> mediaExtensions = new[]
            { ".jpeg", ".bmp", ".jpg", ".png", ".gif", ".webp", ".mp4", ".mov", ".jfif", ".ico", ".mp3" };

        if (File.Exists(ExtensionsFile))
        {
            mediaExtensions =
                JsonSerializer.Deserialize<IEnumerable<string>>(await File.ReadAllTextAsync(ExtensionsFile)) ??
                new string[] { };

            Console.WriteLine($"Loaded {ExtensionsFile} with {mediaExtensions.Count()} extensions");
        }

        foreach (var dirPath in pathsInFileContainingFilePath)
        {
            var loopFileCount = 0;

            foreach (var filePath in Directory.GetFiles(dirPath, "*.*", SearchOption.AllDirectories))
            {
                string ext = Path.GetExtension(filePath);

                if (encryptOrDecryptChoice?.ToLower() == "e" && mediaExtensions.Contains(ext.ToLower()) &&
                    !ext.Contains("-")) // Encrypt
                {
                    loopFileCount++;
                    fileCount++;

                    string newFileName = Path.GetFileNameWithoutExtension(filePath) + "." +
                                         new string(ext.Reverse().ToArray()).Replace(".", "") +
                                         "-";

                    string newPath = Path.Combine(Path.GetDirectoryName(filePath), newFileName);
                    int tries = 0;

                    do
                    {
                        if (!File.Exists(newPath))
                            File.Move(filePath, newPath);

                        newFileName = Path.GetFileNameWithoutExtension(filePath) + $"-{tries}." +
                                      new string(ext.Reverse().ToArray()).Replace(".", "") +
                                      "-";

                        newPath = Path.Combine(Path.GetDirectoryName(filePath), newFileName);

                        tries++;
                    } while (File.Exists(newPath));
                }
                else if (encryptOrDecryptChoice.ToLower() == "d" && ext.Contains("-")) // Decrypt
                {
                    loopFileCount++;
                    fileCount++;

                    var reversedExt = new string(ext.TrimEnd('-').Trim('.').Reverse().ToArray());

                    if (mediaExtensions.Any(x => reversedExt.Contains(x.Trim('.'))))
                    {
                        string newFileName = Path.GetFileNameWithoutExtension(filePath) + "." + reversedExt;
                        string newPath = Path.Combine(Path.GetDirectoryName(filePath), newFileName);


                        if (File.Exists(newPath))
                        {
                            Console.WriteLine(
                                $"File already exists: {newPath.Pastel(Color.PaleVioletRed)} deleting it");
                            File.Delete(filePath);
                        }

                        if (!File.Exists(newPath))
                            File.Move(filePath, newPath);
                    }
                }
            }

            Console.WriteLine(
                $"Processed {loopFileCount.ToString().Pastel(Color.SpringGreen)} in {dirPath.Pastel(Color.Coral)}");
        }

        Console.WriteLine($"Done, {fileCount.ToString().Pastel(Color.SpringGreen)} total files processed");

        Console.WriteLine(
            $"{(encryptOrDecryptChoice.Trim().Equals("e", StringComparison.InvariantCultureIgnoreCase) ? "Encryption" : "Decryption")} done!");
    }
}

// Pastel 

#region Pastel

namespace Pastel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Controls colored console output by <see langword="Pastel"/>.
    /// </summary>
#if NET7_0_OR_GREATER
    public static partial class ConsoleExtensions
#else
    public static class ConsoleExtensions
#endif
    {
        private const string Kernel32DllName = "kernel32";

        private const int STD_OUTPUT_HANDLE = -11;
        private const uint ENABLE_PROCESSED_OUTPUT = 0x0001;
        private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

#if NET7_0_OR_GREATER
        [LibraryImport(Kernel32DllName, EntryPoint = "GetConsoleMode")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool GetConsoleMode(nint hConsoleHandle, out uint lpMode);

        [LibraryImport(Kernel32DllName, EntryPoint = "SetConsoleMode")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetConsoleMode(nint hConsoleHandle, uint dwMode);

        [LibraryImport(Kernel32DllName, EntryPoint = "GetStdHandle")]
        private static partial nint GetStdHandle(int nStdHandle);
#else
        [DllImport(Kernel32DllName)]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport(Kernel32DllName)]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport(Kernel32DllName)]
        private static extern IntPtr GetStdHandle(int nStdHandle);
#endif

        private static bool _enabled;

        private static readonly IReadOnlyDictionary<ConsoleColor, Color> _consoleColorMapper =
            new ReadOnlyDictionary<ConsoleColor, Color>(new Dictionary<ConsoleColor, Color>
            {
                [ConsoleColor.Black] = Color.FromArgb(0x000000),
                [ConsoleColor.DarkBlue] = Color.FromArgb(0x00008B),
                [ConsoleColor.DarkGreen] = Color.FromArgb(0x006400),
                [ConsoleColor.DarkCyan] = Color.FromArgb(0x008B8B),
                [ConsoleColor.DarkRed] = Color.FromArgb(0x8B0000),
                [ConsoleColor.DarkMagenta] = Color.FromArgb(0x8B008B),
                [ConsoleColor.DarkYellow] = Color.FromArgb(0x808000),
                [ConsoleColor.Gray] = Color.FromArgb(0x808080),
                [ConsoleColor.DarkGray] = Color.FromArgb(0xA9A9A9),
                [ConsoleColor.Blue] = Color.FromArgb(0x0000FF),
                [ConsoleColor.Green] = Color.FromArgb(0x008000),
                [ConsoleColor.Cyan] = Color.FromArgb(0x00FFFF),
                [ConsoleColor.Red] = Color.FromArgb(0xFF0000),
                [ConsoleColor.Magenta] = Color.FromArgb(0xFF00FF),
                [ConsoleColor.Yellow] = Color.FromArgb(0xFFFF00),
                [ConsoleColor.White] = Color.FromArgb(0xFFFFFF)
            });

        private const char _fgColorPlaneFormatModifierInitialPart = '3';
        private const char _bgColorPlaneFormatModifierInitialPart = '4';

        private static readonly char[] s_singleDigitCharCache = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        // Values for '\0' to 'f' where 255 indicates invalid input character
        // Starting from '\0' and not from '0' costs 48 bytes but results in zero subtractions and less if statements
        private static readonly byte[] s_fromHexTable =
        {
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 0, 1,
            2, 3, 4, 5, 6, 7, 8, 9, 255, 255,
            255, 255, 255, 255, 255, 10, 11, 12, 13, 14,
            15, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 10, 11, 12,
            13, 14, 15
        };

        // Same as above but valid values are multiplied by 16
        private static readonly byte[] s_fromHexTable16 =
        {
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 0, 16,
            32, 48, 64, 80, 96, 112, 128, 144, 255, 255,
            255, 255, 255, 255, 255, 160, 176, 192, 208, 224,
            240, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 160, 176, 192,
            208, 224, 240
        };

        static ConsoleExtensions()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var iStdOut = GetStdHandle(STD_OUTPUT_HANDLE);
                var _ = GetConsoleMode(iStdOut, out var outConsoleMode)
                        && SetConsoleMode(iStdOut,
                            outConsoleMode | ENABLE_PROCESSED_OUTPUT | ENABLE_VIRTUAL_TERMINAL_PROCESSING);
            }


            if (EnvironmentDetector.ColorsEnabled())
            {
                Enable();
            }
            else
            {
                Disable();
            }
        }

        /// <summary>
        /// Enables any future console color output produced by Pastel.
        /// </summary>
        public static void Enable()
        {
            _enabled = true;
        }

        /// <summary>
        /// Disables any future console color output produced by Pastel.
        /// </summary>
        public static void Disable()
        {
            _enabled = false;
        }

        /// <summary>
        /// Returns a string wrapped in an ANSI foreground color code using the specified color.
        /// </summary>
        /// <param name="input">The string to color.</param>
        /// <param name="color">The color to use on the specified string.</param>
        public static string Pastel(this string input, Color color)
        {
            if (_enabled)
            {
                return PastelInternal(in input, color.R, color.G, color.B, _fgColorPlaneFormatModifierInitialPart);
            }

            return input;
        }

        /// <summary>
        /// Returns a string wrapped in an ANSI foreground color code using the specified color.
        /// </summary>
        /// <param name="input">The string to color.</param>
        /// <param name="color">The color to use on the specified string.</param>
        public static string Pastel(this string input, ConsoleColor color)
        {
            return Pastel(input, _consoleColorMapper[color]);
        }

        /// <summary>
        /// Returns a string wrapped in an ANSI foreground color code using the specified color.
        /// </summary>
        /// <param name="input">The string to color.</param>
        /// <param name="hexColor">The color to use on the specified string.<para>Supported format: [#]RRGGBB (case-insensitive).</para></param>
        public static string Pastel(this string input, in string hexColor)
        {
            if (_enabled)
            {
#if NET6_0_OR_GREATER
                HexToRgb(hexColor, out var r, out var g, out var b);
#else
                HexToRgb(hexColor.AsSpan(), out var r, out var g, out var b);
#endif

                return PastelInternal(in input, r, g, b, _fgColorPlaneFormatModifierInitialPart);
            }

            return input;
        }

        /// <summary>
        /// Returns a string wrapped in an ANSI background color code using the specified color.
        /// </summary>
        /// <param name="input">The string to color.</param>
        /// <param name="color">The color to use on the specified string.</param>
        public static string PastelBg(this string input, Color color)
        {
            if (_enabled)
            {
                return PastelInternal(in input, color.R, color.G, color.B, _bgColorPlaneFormatModifierInitialPart);
            }

            return input;
        }

        /// <summary>
        /// Returns a string wrapped in an ANSI background color code using the specified color.
        /// </summary>
        /// <param name="input">The string to color.</param>
        /// <param name="color">The color to use on the specified string.</param>
        public static string PastelBg(this string input, ConsoleColor color)
        {
            return PastelBg(input, _consoleColorMapper[color]);
        }

        /// <summary>
        /// Returns a string wrapped in an ANSI background color code using the specified color.
        /// </summary>
        /// <param name="input">The string to color.</param>
        /// <param name="hexColor">The color to use on the specified string.<para>Supported format: [#]RRGGBB (case-insensitive).</para></param>
        public static string PastelBg(this string input, string hexColor)
        {
            if (_enabled)
            {
#if NET6_0_OR_GREATER
                HexToRgb(hexColor, out var r, out var g, out var b);
#else
                HexToRgb(hexColor.AsSpan(), out var r, out var g, out var b);
#endif

                return PastelInternal(in input, r, g, b, _bgColorPlaneFormatModifierInitialPart);
            }

            return input;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string PastelInternal(in string input, byte r, byte g, byte b,
            char colorPlaneFormatModifierInitialPart)
        {
            var inputSpan = input.AsSpan();

            var rDigitCount = CountDigits(r);
            var gDigitCount = CountDigits(g);
            var bDigitCount = CountDigits(b);

            var formatStringStartColorInsertCount = 0;

            var haystack = inputSpan;

            // We're looking for all escape sequence resets (\x[0m) NOT followed by either 1) another reset or 2) a color escape sequence and are NOT at the end of the input string
            // In summary, all solitary escape sequence resets between the start and end of the input string

            int pos;
#if NET6_0_OR_GREATER
            while ((pos = haystack.IndexOf("\x1b[0m")) >= 0)
#else
            while ((pos = haystack.IndexOf("\x1b[0m".AsSpan())) >= 0)
#endif
            {
                haystack = haystack.Slice(pos + 4);

                if (haystack.Length > 0)
                {
#if NET8_0_OR_GREATER
                    if (haystack is not ['\x1b', '[', ..])
                    {
                        formatStringStartColorInsertCount++;
                    }
                    else
                    {
                        if (haystack is not [_, _, '3', '8', ..] and not [_, _, '4', '8', ..] and not
                            [_, _, '0', 'm', ..])
                        {
                            formatStringStartColorInsertCount++;
                        }
                    }
#elif NET6_0_OR_GREATER
                    if (!haystack.StartsWith("\x1b["))
                    {
                        formatStringStartColorInsertCount++;
                    }
                    else
                    {
                        var haystackSlice = haystack.Slice(2, 2);

                        if (!haystackSlice.SequenceEqual("38") && !haystackSlice.SequenceEqual("48") && !haystackSlice.SequenceEqual("0m")) {
                            formatStringStartColorInsertCount++;
                        }
                    }
#else
                    if (!haystack.StartsWith("\x1b[".AsSpan()))
                    {
                        formatStringStartColorInsertCount++;
                    }
                    else
                    {
                        var haystackSlice = haystack.Slice(2, 2);

                        if (!haystackSlice.SequenceEqual("38".AsSpan()) && !haystackSlice.SequenceEqual("48".AsSpan()) && !haystackSlice.SequenceEqual("0m".AsSpan())) {
                            formatStringStartColorInsertCount++;
                        }
                    }
#endif
                }
            }

            var colorFormatStringInsertPositions = new int[formatStringStartColorInsertCount];

            if (formatStringStartColorInsertCount > 0)
            {
                formatStringStartColorInsertCount = 0;

                haystack = inputSpan;

                int offsetPos = 0;
#if NET6_0_OR_GREATER
                while ((pos = haystack.IndexOf("\x1b[0m")) >= 0)
#else
                while ((pos = haystack.IndexOf("\x1b[0m".AsSpan())) >= 0)
#endif
                {
                    haystack = haystack.Slice(pos + 4);
                    offsetPos += pos + 4;

                    if (haystack.Length > 0)
                    {
#if NET8_0_OR_GREATER
                        if (haystack is not ['\x1b', '[', ..])
                        {
                            colorFormatStringInsertPositions[formatStringStartColorInsertCount++] = offsetPos;
                        }
                        else
                        {
                            if (haystack is not [_, _, '3', '8', ..] and not [_, _, '4', '8', ..] and not
                                [_, _, '0', 'm', ..])
                            {
                                colorFormatStringInsertPositions[formatStringStartColorInsertCount++] = offsetPos;
                            }
                        }
#elif NET6_0_OR_GREATER
                    if (!haystack.StartsWith("\x1b["))
                    {
                        colorFormatStringInsertPositions[formatStringStartColorInsertCount++] = offsetPos;
                    }
                    else
                    {
                        var haystackSlice = haystack.Slice(2, 2);

                        if (!haystackSlice.SequenceEqual("38") && !haystackSlice.SequenceEqual("48") && !haystackSlice.SequenceEqual("0m")) {
                            colorFormatStringInsertPositions[formatStringStartColorInsertCount++] = offsetPos;
                        }
                    }
#else
                        if (!haystack.StartsWith("\x1b[".AsSpan()))
                        {
                            colorFormatStringInsertPositions[formatStringStartColorInsertCount++] = offsetPos;
                        }
                        else
                        {
                            var haystackSlice = haystack.Slice(2, 2);

                            if (!haystackSlice.SequenceEqual("38".AsSpan()) && !haystackSlice.SequenceEqual("48".AsSpan()) && !haystackSlice.SequenceEqual("0m".AsSpan())) {
                                colorFormatStringInsertPositions[formatStringStartColorInsertCount++] = offsetPos;
                            }
                        }
#endif
                    }
                }
            }

#if NET8_0_OR_GREATER
            var addResetSuffix = inputSpan is not [.., '\x1b', '[', '0', 'm'];
#elif NET6_0_OR_GREATER
            var addResetSuffix = !inputSpan.EndsWith("\x1b[0m");
#else
            var addResetSuffix = !inputSpan.EndsWith("\x1b[0m".AsSpan());
#endif
            var bufferLength =
                (7 + rDigitCount + 1 + gDigitCount + 1 + bDigitCount + 1) *
                (1 + colorFormatStringInsertPositions.Length) + input.Length + (addResetSuffix ? 4 : 0);

#if NET7_0_OR_GREATER
            var stringCreateData = new StringCreateData
            {
                ColorPlaneFormatModifierInitialPart = colorPlaneFormatModifierInitialPart,
                R = r,
                RDigitCount = rDigitCount,
                G = g,
                GDigitCount = gDigitCount,
                B = b,
                BDigitCount = bDigitCount,
                AddResetSuffix = addResetSuffix
            };
#elif NET6_0_OR_GREATER
            var stringCreateData = new StringCreateData(colorPlaneFormatModifierInitialPart,
                                                        r,
                                                        rDigitCount,
                                                        g,
                                                        gDigitCount,
                                                        b,
                                                        bDigitCount,
                                                        addResetSuffix);
#else
            return PastelInternal(bufferLength,
                                  input.AsSpan(),
                                  colorPlaneFormatModifierInitialPart,
                                  r,
                                  rDigitCount,
                                  g,
                                  gDigitCount,
                                  b,
                                  bDigitCount,
                                  colorFormatStringInsertPositions,
                                  addResetSuffix);
#endif

#if NET6_0_OR_GREATER
            return PastelInternal(bufferLength, input, in stringCreateData, colorFormatStringInsertPositions);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if NET6_0_OR_GREATER
        private static unsafe string PastelInternal(int bufferLength, string input,
            in StringCreateData stringCreateData, int[] colorFormatStringInsertPositions)
        {
            string pastelString;

            fixed (StringCreateData* stringCreateDataPtr = &stringCreateData)
            {
                pastelString = string.Create(bufferLength,
                    (
                        Ptr: (nint)stringCreateDataPtr,
                        Text: input,
                        ColorFormatStringInsertPositions: colorFormatStringInsertPositions
                    ),
                    static (buf, ctx) =>
                    {
                        var ctxPtr = *(StringCreateData*)ctx.Ptr;

                        int i = 1;

                        buf[0] = '\x1b';
                        buf[i++] = '[';
                        buf[i++] = ctxPtr.ColorPlaneFormatModifierInitialPart;
                        buf[i++] = '8';
                        buf[i++] = ';';
                        buf[i++] = '2';
                        buf[i++] = ';';

                        ByteToString(buf.Slice(i, ctxPtr.RDigitCount), ctxPtr.R);
                        i += ctxPtr.RDigitCount;

                        buf[i++] = ';';

                        ByteToString(buf.Slice(i, ctxPtr.GDigitCount), ctxPtr.G);
                        i += ctxPtr.GDigitCount;

                        buf[i++] = ';';

                        ByteToString(buf.Slice(i, ctxPtr.BDigitCount), ctxPtr.B);
                        i += ctxPtr.BDigitCount;

                        buf[i++] = 'm';


                        var colorFormatStringBuf = buf.Slice(0, i);
                        var textSpan = ctx.Text.AsSpan();
                        var currentIndex = i;
                        if (ctx.ColorFormatStringInsertPositions.Length > 0)
                        {
                            int previousInsertPos = 0;

                            for (var colorFormatStringIndex = 0;
                                 colorFormatStringIndex < ctx.ColorFormatStringInsertPositions.Length;
                                 colorFormatStringIndex++)
                            {
                                var currentInsertPos = ctx.ColorFormatStringInsertPositions[colorFormatStringIndex];
                                var segmentLength = currentInsertPos - previousInsertPos;

                                CopySegmentToBuffer(textSpan.Slice(previousInsertPos, segmentLength), buf,
                                    ref currentIndex);
                                CopySegmentToBuffer(colorFormatStringBuf, buf, ref currentIndex);

                                previousInsertPos = currentInsertPos;
                            }

                            CopySegmentToBuffer(textSpan.Slice(previousInsertPos), buf, ref currentIndex);
                        }
                        else
                        {
                            CopySegmentToBuffer(textSpan, buf, ref currentIndex);
                        }

                        if (ctxPtr.AddResetSuffix)
                        {
                            buf[currentIndex++] = '\x1b';
                            buf[currentIndex++] = '[';
                            buf[currentIndex++] = '0';
                            buf[currentIndex] = 'm';
                        }
                    });
            }

            return pastelString;
        }
#else
        private static string PastelInternal(int bufferLength, in ReadOnlySpan<char> input, char colorPlaneFormatModifierInitialPart, byte r, byte rDigitCount, byte g, byte gDigitCount, byte b, byte bDigitCount, int[] colorFormatStringInsertPositions, bool addResetSuffix)
        {
            var buf = new char[bufferLength];
            var bufSpan = buf.AsSpan();

            int i = 1;

            bufSpan[0] = '\x1b';
            bufSpan[i++] = '[';
            bufSpan[i++] = colorPlaneFormatModifierInitialPart;
            bufSpan[i++] = '8';
            bufSpan[i++] = ';';
            bufSpan[i++] = '2';
            bufSpan[i++] = ';';

            ByteToString(bufSpan.Slice(i, rDigitCount), r);
            i += rDigitCount;

            bufSpan[i++] = ';';

            ByteToString(bufSpan.Slice(i, gDigitCount), g);
            i += gDigitCount;

            bufSpan[i++] = ';';

            ByteToString(bufSpan.Slice(i, bDigitCount), b);
            i += bDigitCount;

            bufSpan[i++] = 'm';


            var colorFormatStringBuf = bufSpan.Slice(0, i);
            var currentIndex = i;
            if (colorFormatStringInsertPositions.Length > 0)
            {
                int previousInsertPos = 0;

                for (var colorFormatStringIndex =
 0; colorFormatStringIndex < colorFormatStringInsertPositions.Length; colorFormatStringIndex++)
                {
                    var currentInsertPos = colorFormatStringInsertPositions[colorFormatStringIndex];
                    var segmentLength = currentInsertPos - previousInsertPos;

                    CopySegmentToBuffer(input.Slice(previousInsertPos, segmentLength), buf, ref currentIndex);
                    CopySegmentToBuffer(colorFormatStringBuf, buf, ref currentIndex);

                    previousInsertPos = currentInsertPos;
                }

                CopySegmentToBuffer(input.Slice(previousInsertPos), buf, ref currentIndex);
            }
            else
            {
                CopySegmentToBuffer(input, buf, ref currentIndex);
            }

            if (addResetSuffix)
            {
                bufSpan[currentIndex++] = '\x1b';
                bufSpan[currentIndex++] = '[';
                bufSpan[currentIndex++] = '0';
                bufSpan[currentIndex] = 'm';
            }

            return new string(buf);
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CopySegmentToBuffer(in ReadOnlySpan<char> segment, Span<char> destination,
            ref int currentIndex)
        {
            segment.CopyTo(destination.Slice(currentIndex));
            currentIndex += segment.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte CountDigits(byte value)
        {
            byte digits = 1;

            if (value < 10)
            {
                // no-op
            }
            else if (value < 100)
            {
                digits++;
            }
            else
            {
                digits += 2;
            }

            return digits;
        }

#if NET6_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ByteToString(Span<char> buffer, byte value)
        {
            if (buffer.Length == 1)
            {
                var singleDigitCharCache = s_singleDigitCharCache;
                buffer[0] = singleDigitCharCache[value];

                return;
            }

            var i = buffer.Length;
            do
            {
                (value, var remainder) = Math.DivRem(value, (byte)10);
                buffer[--i] = (char)(remainder + '0');
            } while (value != 0);
        }
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ByteToString(Span<char> buffer, int value)
        {
            if (buffer.Length == 1)
            {
                var singleDigitCharCache = s_singleDigitCharCache;
                buffer[0] = singleDigitCharCache[value];

                return;
            }

            var i = buffer.Length;
            do
            {
                value = Math.DivRem(value, 10, out var remainder);
                buffer[--i] = (char)(remainder + '0');
            }
            while (value != 0);
        }
#endif

        private static void HexToRgb(ReadOnlySpan<char> hexString, out byte r, out byte g, out byte b)
        {
#if NET8_0_OR_GREATER
            if (hexString is ['#', ..])
#else
            if (hexString.Length > 1 && hexString[0] == '#')
#endif
            {
                hexString = hexString.Slice(1);
            }

            switch (hexString.Length)
            {
                case 3:
                    r = FromChar(in hexString[0], in hexString[0], in hexString);
                    g = FromChar(in hexString[1], in hexString[1], in hexString);
                    b = FromChar(in hexString[2], in hexString[2], in hexString);

                    return;
                case 6:
                    r = FromChar(in hexString[0], in hexString[1], in hexString);
                    g = FromChar(in hexString[2], in hexString[3], in hexString);
                    b = FromChar(in hexString[4], in hexString[5], in hexString);

                    return;
                default:
                    throw InvalidHexadecimalColorValueException(hexString.ToString());
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte FromChar(in char byteHi, in char byteLo, in ReadOnlySpan<char> hexString)
        {
            byte result, add;

            if (byteHi > 102
                || (result = s_fromHexTable16[byteHi]) == 255
                || byteLo > 102
                || (add = s_fromHexTable[byteLo]) == 255
               )
            {
                throw InvalidHexadecimalColorValueException(hexString.ToString());
            }

            return (byte)(result + add);
        }

#if NET6_0_OR_GREATER
        private static ArgumentException InvalidHexadecimalColorValueException(in ReadOnlySpan<char> hexString) =>
            new($"Invalid hexadecimal color value encountered: {hexString}", nameof(hexString));
#else
        private static ArgumentException InvalidHexadecimalColorValueException(string hexString) => new ArgumentException("Invalid hexadecimal color value encountered: " + hexString, nameof(hexString));
#endif

#if NET7_0_OR_GREATER
        private readonly struct StringCreateData
        {
            public required char ColorPlaneFormatModifierInitialPart { get; init; }

            public required byte R { get; init; }

            public required byte RDigitCount { get; init; }

            public required byte G { get; init; }

            public required byte GDigitCount { get; init; }

            public required byte B { get; init; }

            public required byte BDigitCount { get; init; }

            public required bool AddResetSuffix { get; init; }
        }
#else
        private readonly struct StringCreateData
        {
            public char ColorPlaneFormatModifierInitialPart { get; }

            public byte R { get; }

            public byte RDigitCount { get; }

            public byte G { get; }

            public byte GDigitCount { get; }

            public byte B { get; }

            public byte BDigitCount { get; }

            public bool AddResetSuffix { get; }


            internal StringCreateData(char colorPlaneFormatModifierInitialPart, byte r, byte rDigitCount, byte g, byte gDigitCount, byte b, byte bDigitCount, bool addResetSuffix)
            {
                ColorPlaneFormatModifierInitialPart = colorPlaneFormatModifierInitialPart;
                R = r;
                RDigitCount = rDigitCount;
                G = g;
                GDigitCount = gDigitCount;
                B = b;
                BDigitCount = bDigitCount;
                AddResetSuffix = addResetSuffix;
            }
        }
#endif
    }
}

#endregion


#region Environment Detector

namespace Pastel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    internal static class EnvironmentDetector
    {
        private const string DisableEnvironmentDetectionEnvironmentVariableName =
            "PASTEL_DISABLE_ENVIRONMENT_DETECTION";

        /// <summary>
        /// Returns <see langword="true"/> if at least one of a predefined set of environment variables are set. These environment variables could e.g. indicate that the application is running in a CI/CD environment.
        /// </summary>
        public static bool ColorsEnabled()
        {
            return Environment.GetEnvironmentVariable(DisableEnvironmentDetectionEnvironmentVariableName) != null ||
                   !HasEnvironmentVariable();
        }

#if NET7_0_OR_GREATER
        private static readonly Func<string, string, bool>[] s_environmentVariableDetectors =
        [
            IsBitbucketEnvironmentVariableKey,
            IsTeamCityEnvironmentVariableKey,
            NoColor,
            IsGitHubAction,
            IsCI,
            IsJenkins
        ];
#else
        private static readonly Func<string, string, bool>[] s_environmentVariableDetectors = {
                                                                                                 IsBitbucketEnvironmentVariableKey,
                                                                                                 IsTeamCityEnvironmentVariableKey,
                                                                                                 NoColor,
                                                                                                 IsGitHubAction,
                                                                                                 IsCI,
                                                                                                 IsJenkins
                                                                                              };
#endif

        private static bool IsBitbucketEnvironmentVariableKey(string key, string value)
        {
            return key.StartsWith("BITBUCKET_", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsTeamCityEnvironmentVariableKey(string key, string value)
        {
            return key.StartsWith("TEAMCITY_", StringComparison.OrdinalIgnoreCase);
        }

        // https://no-color.org/
        private static bool NoColor(string key, string value)
        {
            return key.Equals("NO_COLOR", StringComparison.OrdinalIgnoreCase);
        }

        // Set by GitHub Actions
        private static bool IsGitHubAction(string key, string value)
        {
            return key.StartsWith("GITHUB_ACTION", StringComparison.OrdinalIgnoreCase);
        }

        // Set by GitHub Actions and Travis CI
        private static bool IsCI(string key, string value)
        {
            return key.Equals("CI", StringComparison.OrdinalIgnoreCase)
                   && (value.Equals("true", StringComparison.OrdinalIgnoreCase) || value == "1");
        }

        // Detect Jenkins enviroment
        private static bool IsJenkins(string key, string value)
        {
            return key.StartsWith("JENKINS_URL", StringComparison.OrdinalIgnoreCase);
        }

        private static bool HasEnvironmentVariable()
        {
            var environmentVariables = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process)
                .Cast<DictionaryEntry>()
                .Concat(Environment.GetEnvironmentVariables(EnvironmentVariableTarget.User).Cast<DictionaryEntry>())
                .Concat(Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Machine).Cast<DictionaryEntry>())
                .Select(de =>
                    new KeyValuePair<string, string>(de.Key.ToString(), de.Value != null ? de.Value.ToString() : ""))
                .GroupBy(kvp => kvp.Key, (_, kvps) => kvps.First())
                .ToList();

            foreach (var environmentVariable in environmentVariables)
            {
                if (s_environmentVariableDetectors.Any(evd => evd(environmentVariable.Key, environmentVariable.Value)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

#endregion