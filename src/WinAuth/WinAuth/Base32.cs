/*
 * Copyright (C) 2011 Colin Mackie.
 * This software is distributed under the terms of the GNU General Public License.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

namespace WinAuth.WinAuth;

/// <summary>
/// 实现向 Base32 RFC3548 转换的类
/// </summary>
public sealed partial class Base32
{
    /// <summary>
    /// 默认 base32 字符集，按照 RFC 4648/3548
    /// </summary>
    const string DefaultAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    /// <summary>
    /// 查找右边的零位数(例如 0100 = 2)
    /// </summary>
    static readonly int[] NumberTrailingZerosLookup =
    {
        32, 0, 1, 26, 2, 23, 27, 0, 3, 16, 24, 30, 28, 11, 0, 13, 4, 7, 17,
        0, 25, 22, 31, 15, 29, 10, 12, 6, 0, 21, 14, 9, 5, 20, 8, 19, 18,
    };

    /// <summary>
    /// 单例实例
    /// </summary>
    static readonly Base32 _instance = new(DefaultAlphabet);

    /// <summary>
    /// 获取一个 Base32 对象的实例，可以是标准的单例对象，也可以是自定义字母
    /// </summary>
    /// <param name="alphabet"></param>
    /// <returns></returns>
    public static Base32 GetInstance(string? alphabet = null)
    {
        return alphabet == null ? _instance : new Base32(alphabet);
    }

    /// <summary>
    /// 字母表字符的数组
    /// </summary>
    readonly char[] _digits;

    /// <summary>
    /// 使用的位掩码
    /// </summary>
    readonly int _mask;

    /// <summary>
    /// 变化的值
    /// </summary>
    readonly int _shift;

    /// <summary>
    /// 字符到位置的映射
    /// </summary>
    readonly Dictionary<char, int> _map;

    /// <summary>
    /// 用指定的字母表创建一个新的 Base32 对象
    /// </summary>
    /// <param name="alphabet"></param>
    Base32(string alphabet)
    {
        // initialise the decoder and precalculate the char map
        _digits = alphabet.ToCharArray();
        _mask = _digits.Length - 1;
        _shift = NumberOfTrailingZeros(_digits.Length);
        _map = [];
        for (var i = 0; i < _digits.Length; i++)
            _map.Add(_digits[i], i);
    }

    /// <summary>
    /// 计算右侧尾位为零的位数(例如 0100 = 2)
    /// http://graphics.stanford.edu/~seander/bithacks.html#ZerosOnRightModLookup
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    static int NumberOfTrailingZeros(int i)
    {
        return NumberTrailingZerosLookup[(i & -i) % 37];
    }

    /// <summary>
    /// 数据解码
    /// </summary>
    /// <param name="encoded"></param>
    /// <returns></returns>
    /// <exception cref="WinAuthBase32DecodingException"></exception>
    public byte[] Decode(string encoded)
    {
        // remove whitespace and any separators
        encoded = RemoveWhitespaceAndAnySeparatorsRegex().Replace(encoded, "");

        // Google implementation ignores padding
        encoded = GoogleImplIgnoresPaddingRegex().Replace(encoded, "");

        // convert as uppercase
        encoded = encoded.ToUpper(CultureInfo.InvariantCulture);

        // handle zero case
        if (encoded.Length == 0)
            return Array.Empty<byte>();

        var encodedLength = encoded.Length;
        var outLength = encodedLength * _shift / 8;
        var result = new byte[outLength];
        var buffer = 0;
        var next = 0;
        var bitsLeft = 0;
        foreach (var c in encoded.ToCharArray())
        {
            if (_map.ContainsKey(c) == false)
                throw new WinAuthBase32DecodingException("Illegal character: " + c);
            buffer <<= _shift;
            buffer |= _map[c] & _mask;
            bitsLeft += _shift;
            if (bitsLeft >= 8)
            {
                result[next++] = (byte)(buffer >> (bitsLeft - 8));
                bitsLeft -= 8;
            }
        }
        // We'll ignore leftover bits for now.
        //
        // if (next != outLength || bitsLeft >= SHIFT) {
        //  throw new DecodingException("Bits left: " + bitsLeft);
        // }

        return result;
    }

    /// <summary>
    /// 数据编码
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public string Encode(byte[] data)
    {
        if (data.Length == 0)
            return string.Empty;

        /*
                    // _shift is the number of bits per output character, so the length of the
                    // output is the length of the input multiplied by 8/_shift, rounded up.
                    if (data.Length >= (1 << 28))
                    {
                        // The computation below will fail, so don't do it.
                        throw new IllegalArgumentException();
                    }

                    // calculate result length
                    int outputLength = (data.Length * 8 + _shift - 1) / _shift;
                    StringBuilder result = new StringBuilder(outputLength);
        */

        var result = new StringBuilder();

        // encode data and map chars into result buffer
        int buffer = data[0];
        var next = 1;
        var bitsLeft = 8;
        while (bitsLeft > 0 || next < data.Length)
        {
            if (bitsLeft < _shift)
                if (next < data.Length)
                {
                    buffer <<= 8;
                    buffer |= data[next++] & 0xff;
                    bitsLeft += 8;
                }
                else
                {
                    var pad = _shift - bitsLeft;
                    buffer <<= pad;
                    bitsLeft += pad;
                }
            var index = _mask & buffer >> (bitsLeft - _shift);
            bitsLeft -= _shift;
            result.Append(_digits[index]);
        }

        return result.ToString();
    }

    [GeneratedRegex("[\\s-]+")]
    private static partial Regex RemoveWhitespaceAndAnySeparatorsRegex();

    [GeneratedRegex("[=]*$")]
    private static partial Regex GoogleImplIgnoresPaddingRegex();
}