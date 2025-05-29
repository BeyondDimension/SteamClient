// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// https://github.com/dotnet/aspnetcore/blob/v9.0.5/src/Identity/Extensions.Core/src/Base32.cs

using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace BD.SteamClient8.WinAuth.Helpers;

// See http://tools.ietf.org/html/rfc3548#section-5
public static partial class Base32
{
    const string _base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

#if NET6_0_OR_GREATER
    public static string GenerateBase32()
    {
        const int length = 20;
        // base32 takes 5 bytes and converts them into 8 characters, which would be (byte length / 5) * 8
        // except that it also pads ('=') for the last processed chunk if it's less than 5 bytes.
        // So in order to handle the padding we add 1 less than the chunk size to our byte length
        // which will either be removed due to integer division truncation if the length was already a multiple of 5
        // or it will increase the divided length by 1 meaning that a 1-4 byte length chunk will be 1 instead of 0
        // so the padding is now included in our string length calculation
        return string.Create((length + 4) / 5 * 8, 0, static (buffer, _) =>
        {
            Span<byte> bytes = stackalloc byte[length];
            RandomNumberGenerator.Fill(bytes);

            var index = 0;
            for (var offset = 0; offset < bytes.Length;)
            {
                byte a, b, c, d, e, f, g, h;
                var numCharsToOutput = GetNextGroup(bytes, ref offset, out a, out b, out c, out d, out e, out f, out g, out h);

                if (numCharsToOutput >= 8) buffer[index + 7] = _base32Chars[h];
                if (numCharsToOutput >= 7) buffer[index + 6] = _base32Chars[g];
                if (numCharsToOutput >= 6) buffer[index + 5] = _base32Chars[f];
                if (numCharsToOutput >= 5) buffer[index + 4] = _base32Chars[e];
                if (numCharsToOutput >= 4) buffer[index + 3] = _base32Chars[d];
                if (numCharsToOutput >= 3) buffer[index + 2] = _base32Chars[c];
                if (numCharsToOutput >= 2) buffer[index + 1] = _base32Chars[b];
                if (numCharsToOutput >= 1) buffer[index] = _base32Chars[a];
                index += 8;
            }
        });
    }
#endif

    public static string ToBase32(byte[]? input)
    {
        if (input == null || input.Length == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        for (var offset = 0; offset < input.Length;)
        {
            byte a, b, c, d, e, f, g, h;
            var numCharsToOutput = GetNextGroup(input, ref offset, out a, out b, out c, out d, out e, out f, out g, out h);

            sb.Append(numCharsToOutput >= 1 ? _base32Chars[a] : null);
            sb.Append(numCharsToOutput >= 2 ? _base32Chars[b] : null);
            sb.Append(numCharsToOutput >= 3 ? _base32Chars[c] : null);
            sb.Append(numCharsToOutput >= 4 ? _base32Chars[d] : null);
            sb.Append(numCharsToOutput >= 5 ? _base32Chars[e] : null);
            sb.Append(numCharsToOutput >= 6 ? _base32Chars[f] : null);
            sb.Append(numCharsToOutput >= 7 ? _base32Chars[g] : null);
            sb.Append(numCharsToOutput >= 8 ? _base32Chars[h] : null);
        }

        return sb.ToString();
    }

    public static byte[] FromBase32(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return [];
        }

        // Add: 移除空格和任何分隔符，与旧代码保持一致
        input = RemoveWhitespaceAndAnySeparatorsRegex().Replace(input, "");

        var trimmedInput = input.AsSpan().TrimEnd('=');
        if (trimmedInput.Length == 0)
        {
            return [];
        }

        var output = new byte[trimmedInput.Length * 5 / 8];
        var bitIndex = 0;
        var inputIndex = 0;
        var outputBits = 0;
        var outputIndex = 0;
        while (outputIndex < output.Length)
        {
            var byteIndex = _base32Chars.IndexOf(char.ToUpperInvariant(trimmedInput[inputIndex]));
            if (byteIndex < 0)
                throw new FormatException();

            var bits = Math.Min(5 - bitIndex, 8 - outputBits);
            output[outputIndex] <<= bits;
            output[outputIndex] |= (byte)(byteIndex >> 5 - (bitIndex + bits));

            bitIndex += bits;
            if (bitIndex >= 5)
            {
                inputIndex++;
                bitIndex = 0;
            }

            outputBits += bits;
            if (outputBits >= 8)
            {
                outputIndex++;
                outputBits = 0;
            }
        }
        return output;
    }

    // returns the number of bytes that were output
    static int GetNextGroup(Span<byte> input, ref int offset, out byte a, out byte b, out byte c, out byte d, out byte e, out byte f, out byte g, out byte h)
    {
        uint b1, b2, b3, b4, b5;

        int retVal;
        switch (input.Length - offset)
        {
            case 1: retVal = 2; break;
            case 2: retVal = 4; break;
            case 3: retVal = 5; break;
            case 4: retVal = 7; break;
            default: retVal = 8; break;
        }

        b1 = offset < input.Length ? input[offset++] : 0U;
        b2 = offset < input.Length ? input[offset++] : 0U;
        b3 = offset < input.Length ? input[offset++] : 0U;
        b4 = offset < input.Length ? input[offset++] : 0U;
        b5 = offset < input.Length ? input[offset++] : 0U;

        a = (byte)(b1 >> 3);
        b = (byte)((b1 & 0x07) << 2 | b2 >> 6);
        c = (byte)(b2 >> 1 & 0x1f);
        d = (byte)((b2 & 0x01) << 4 | b3 >> 4);
        e = (byte)((b3 & 0x0f) << 1 | b4 >> 7);
        f = (byte)(b4 >> 2 & 0x1f);
        g = (byte)((b4 & 0x3) << 3 | b5 >> 5);
        h = (byte)(b5 & 0x1f);

        return retVal;
    }

    [GeneratedRegex("[\\s-]+")]
    private static partial Regex RemoveWhitespaceAndAnySeparatorsRegex();
}