using System.Buffers;
using System.Extensions;
using System.Globalization;

namespace BD.SteamClient8.Models;

public sealed class SteamIdConvert
{
    // Usage:
    // - SteamIdConvert sid = new SteamIdConvert("STEAM_0:0:52161201");
    //   string Steam64 = sid.Id64;

    const string Default_Id = "STEAM_0:";
    const string Default_Id3 = "U:1:";

    public string Id = Default_Id;
    public string Id3 = Default_Id3;
    public string Id32 = string.Empty;
    public string Id64 = string.Empty;

    private string? _input;
    private byte _inputType;

    private static readonly char[] Sid3Strings = ['U', 'I', 'M', 'G', 'A', 'P', 'C', 'g', 'T', 'L', 'C', 'a'];

    const byte SteamId = 1;
    const byte SteamId3 = 2;
    const byte SteamId32 = 3;
    const byte SteamId64 = 4;

    public const long UndefinedId = 76561197960265728;

    /// <summary>
    /// Initializes a new instance of the <see cref="SteamIdConvert"/> class.
    /// </summary>
    /// <param name="anySteamId"></param>
    public SteamIdConvert(string anySteamId)
    {
        try
        {
            GetIdType(anySteamId);
            ConvertAll();
        }
        catch (Exception ex)
        {
            Log.Error(nameof(SteamIdConvert), ex, "Convert Error");
        }
    }

    void GetIdType(string sInput)
    {
        _input = sInput;
        if (_input[0] == 'S')
        {
            _inputType = 1; // SteamID
        }
        else if (Sid3Strings.Contains(_input[0]))
        {
            _inputType = 2; // SteamID3
        }
        else if (char.IsNumber(_input[0]))
        {
            _inputType = _input.Length switch
            {
                < 17 => 3,
                17 => 4,
                _ => _inputType,
            };
        }
        else
        {
            throw new Exception("Input SteamID was not recognised!");
        }
    }

    static string GetOddity(string input)
    {
        return (int.Parse(input) % 2).ToString();
    }

    static string FloorDivide(string sIn, double divIn)
    {
        return Math.Floor(int.Parse(sIn) / divIn).ToString(CultureInfo.InvariantCulture);
    }

    void CalcSteamId()
    {
        if (_inputType == SteamId)
        {
            Id = _input ?? Default_Id;
        }
        else
        {
            var s = _inputType switch
            {
                SteamId3 => _input == null ? "" : _input[4..],
                SteamId32 => _input,
                SteamId64 => CalcSteamId32(),
                _ => "",
            };

            Id += GetOddity(s!) + ':' + FloorDivide(s!, 2);
        }
    }

    void CalcSteamId3()
    {
        if (_inputType == SteamId3)
        {
            Id3 = _input ?? Default_Id3;
        }
        else
        {
            Id3 += CalcSteamId32();
        }
        Id3 = $"[{Id3}]";
    }

    string CalcSteamId32()
    {
        if (_inputType == SteamId32)
        {
            Id32 = _input ?? string.Empty;
        }
        else
        {
            Id32 = _inputType switch
            {
                SteamId => ((int.Parse(_input.ThrowIsNull()[10..]) * 2) + int.Parse(_input.AsSpan(8, 1))).ToString(),
                SteamId3 => _input.ThrowIsNull()[4..],
                SteamId64 => (long.Parse(_input.ThrowIsNull()) - UndefinedId).ToString(),
                _ => Id32,
            };
        }

        return Id32;
    }

    void CalcSteamId64()
    {
        if (_inputType == SteamId64)
        {
            Id64 = _input ?? string.Empty;
        }
        else
        {
            Id64 = _inputType switch
            {
                SteamId => ((int.Parse(_input.ThrowIsNull()[10..]) * 2) + int.Parse(_input.AsSpan(8, 1)) + UndefinedId).ToString(),
                SteamId3 => (int.Parse(_input.ThrowIsNull()[4..]) + UndefinedId).ToString(),
                SteamId32 => (int.Parse(_input.ThrowIsNull()) + UndefinedId).ToString(),
                _ => Id64,
            };
        }
    }

    public void ConvertAll()
    {
        CalcSteamId();
        CalcSteamId3();
        _ = CalcSteamId32();
        CalcSteamId64();
    }
}
