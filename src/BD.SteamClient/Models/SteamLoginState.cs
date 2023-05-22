namespace BD.SteamClient.Models;

[MPObj, MP2Obj(SerializeLayout.Explicit)]
public sealed partial class SteamLoginState : JsonModel<SteamLoginState>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte[] ToBytes() => Serializable.SMP2(this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SteamLoginState? Parse(byte[] buffer) => Serializable.DMP2<SteamLoginState>(buffer);

    [MPKey(0), MP2Key(0), JsonPropertyOrder(0)]
    public string? Language { get; set; }

    [MPKey(1), MP2Key(1), JsonPropertyOrder(1)]
    public string? Username { get; set; }

    [MPKey(2), MP2Key(2), JsonPropertyOrder(2)]
    public string? Password { get; set; }

    [MPKey(3), MP2Key(3), JsonPropertyOrder(3)]
    public string? CaptchaId { get; set; }

    [MPKey(4), MP2Key(4), JsonPropertyOrder(4)]
    public string? CaptchaUrl { get; set; }

    [MPKey(5), MP2Key(5), JsonPropertyOrder(5)]
    public string? CaptchaImageBase64 { get; set; }

    [MPKey(6), MP2Key(6), JsonPropertyOrder(6)]
    public string? CaptchaText { get; set; }

    [MPKey(7), MP2Key(7), JsonPropertyOrder(7)]
    public string? EmailDomain { get; set; }

    //public string? EmailAuthText { get; set; }

    [MPKey(8), MP2Key(8), JsonPropertyOrder(8)]
    public string? EmailCode { get; set; }

    [MPKey(9), MP2Key(9), JsonPropertyOrder(9)]
    public string? TwofactorCode { get; set; }

    [MessagePackFormatter(typeof(CookieFormatter))]
    [CookieCollectionFormatter]
    [MPKey(10), MP2Key(10), JsonPropertyOrder(10)]
    public CookieCollection? Cookies { get; set; }

    [MPKey(11), MP2Key(11), JsonPropertyOrder(11)]
    public string? SteamIdString { get; set; }

    [MPKey(12), MP2Key(12), JsonPropertyOrder(12)]
    public ulong SteamId { get; set; }

    [MPKey(13), MP2Key(13), JsonPropertyOrder(13)]
    public string? Email { get; set; }

    //public string? OAuthToken { get; set; }

    //public bool RequiresLogin { get; set; }

    [MPKey(14), MP2Key(14), JsonPropertyOrder(14)]
    public bool RequiresCaptcha { get; set; }

    [MPKey(15), MP2Key(15), JsonPropertyOrder(15)]
    public bool Requires2FA { get; set; }

    [MPKey(16), MP2Key(16), JsonPropertyOrder(16)]
    public bool RequiresEmailAuth { get; set; }

    [MPKey(17), MP2Key(17), JsonPropertyOrder(17)]
    public bool Success { get; set; }

    [MPKey(18), MP2Key(18), JsonPropertyOrder(18)]
    public string? Message { get; set; }

    [MPKey(19), MP2Key(19), JsonPropertyOrder(19)]
    public string? WalletBalanceString { get; set; }

    /// <summary>
    /// 疑似未定区账号
    /// </summary>
    [MPKey(20), MP2Key(20), JsonPropertyOrder(20)]
    public bool IsUndeterminedArea { get; set; }

    [MPKey(21), MP2Key(21), JsonPropertyOrder(21)]
    public string? CurrencySymbol { get; set; }

    [MPKey(22), MP2Key(22), JsonPropertyOrder(22)]
    public string? SteamCountry { get; set; }

    [MPKey(23), MP2Key(23), JsonPropertyOrder(23)]
    public int AccountHistoryCount { get; set; }

    [MPKey(24), MP2Key(24), JsonPropertyOrder(24)]
    public string? SeesionId { get; set; }

    [MPKey(25), MP2Key(25), JsonPropertyOrder(25)]
    public ulong? ClientId { get; set; }

    [MPKey(26), MP2Key(26), JsonPropertyOrder(26)]
    public byte[]? RequestId { get; set; }

    public void ResetStatus()
    {
        Requires2FA = false;
        RequiresCaptcha = false;
        RequiresEmailAuth = false;
        Success = false;
        ClientId = null;
        RequestId = null;
        SeesionId = null;
    }
}