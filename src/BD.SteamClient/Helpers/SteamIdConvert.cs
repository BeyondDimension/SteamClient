namespace BD.SteamClient.Helpers;

public class SteamIdConvert
{
    // Usage:
    // - SteamIdConvert sid = new SteamIdConvert("STEAM_0:0:52161201");
    //   string Steam64 = sid.Id64;

#pragma warning disable SA1132 // Do not combine fields
    public string Id = "STEAM_0:", Id3 = "U:1:", Id32 = string.Empty, Id64 = string.Empty;

    private string? _input;
    private byte _inputType;

    private static readonly char[] Sid3Strings = { 'U', 'I', 'M', 'G', 'A', 'P', 'C', 'g', 'T', 'L', 'C', 'a' };
    private const byte SteamId = 1, SteamId3 = 2, SteamId32 = 3, SteamId64 = 4;
    public const long UndefinedId = 76561197960265728;
#pragma warning restore SA1132 // Do not combine fields

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

    private void GetIdType(string sInput)
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
                _ => _inputType
            };
        }
        else
        {
            throw new Exception("Input SteamID was not recognised!");
        }
    }

    private static string GetOddity(string input)
    {
        return (int.Parse(input) % 2).ToString();
    }

    private static string FloorDivide(string sIn, double divIn)
    {
        return Math.Floor(int.Parse(sIn) / divIn).ToString(CultureInfo.InvariantCulture);
    }

    private void CalcSteamId()
    {
        if (_inputType == SteamId)
        {
            Id = _input;
        }
        else
        {
            var s = _inputType switch
            {
                SteamId3 => _input == null ? "" : _input[4..],
                SteamId32 => _input,
                SteamId64 => CalcSteamId32(),
                _ => ""
            };

            Id += GetOddity(s!) + ":" + FloorDivide(s!, 2);
        }
    }

    private void CalcSteamId3()
    {
        if (_inputType == SteamId3)
            Id3 = _input;
        else
            Id3 += CalcSteamId32();
        Id3 = $"[{Id3}]";
    }

    private string CalcSteamId32()
    {
        if (_inputType == SteamId32)
        {
            Id32 = _input;
        }
        else
        {
            Id32 = _inputType switch
            {
                SteamId => ((int.Parse(_input.ThrowIsNull()[10..]) * 2) + int.Parse($"{_input[8]}")).ToString(),
                SteamId3 => _input.ThrowIsNull()[4..],
                SteamId64 => (long.Parse(_input.ThrowIsNull()) - UndefinedId).ToString(),
                _ => Id32
            };
        }

        return Id32;
    }

    private void CalcSteamId64()
    {
        if (_inputType == SteamId64)
            Id64 = _input;
        else
            Id64 = _inputType switch
            {
                SteamId => (int.Parse(this._input.ThrowIsNull()[10..]) * 2 + int.Parse($"{_input[8]}") + UndefinedId).ToString(),
                SteamId3 => (int.Parse(_input.ThrowIsNull()[4..]) + UndefinedId).ToString(),
                SteamId32 => (int.Parse(_input.ThrowIsNull()) + UndefinedId).ToString(),
                _ => Id64
            };
    }

    public void ConvertAll()
    {
        CalcSteamId();
        CalcSteamId3();
        _ = CalcSteamId32();
        CalcSteamId64();
    }
}
