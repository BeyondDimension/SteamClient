using static BD.SteamClient8.Models.AuthenticatorValueModel;

namespace BD.SteamClient8.Models;

public static partial class AuthenticatorExtensions
{
    /// <summary>
    /// 将 <see cref="IAuthenticatorModel"/> 转换为导出模型
    /// </summary>
    /// <param name="this"></param>
    /// <param name="compat"></param>
    /// <returns></returns>
    public static AuthenticatorExportModel ToExport(
        this IAuthenticatorModel @this,
        bool compat = false)
    {
        @this.Value.ThrowIsNull();
        AuthenticatorExportModel m = new();

        var issuer = @this.Value.Issuer;
        var label = @this.Name;
        if (!string.IsNullOrEmpty(issuer))
        {
            m.Issuer = issuer;
        }

        if (@this.Value.HMACType != DEFAULT_HMAC_TYPE)
        {
            m.HMACType = @this.Value.HMACType;
        }

        if (@this.Value is BattleNetAuthenticator battleNetAuthenticator)
        {
            m.Platform = AuthenticatorPlatform.BattleNet;
            m.Serial = battleNetAuthenticator.Serial;
        }
        else if (@this.Value is SteamAuthenticator steamAuthenticator)
        {
            m.Platform = AuthenticatorPlatform.Steam;
            if (!compat)
            {
                m.DeviceId = steamAuthenticator.DeviceId;
                m.SteamData = steamAuthenticator.SteamData;
            }
        }
        else if (@this.Value is HOTPAuthenticator hOTPAuthenticator)
        {
            m.Platform = AuthenticatorPlatform.HOTP;
            m.Counter = hOTPAuthenticator.Counter;
        }
        else if (@this.Value is GoogleAuthenticator)
        {
            m.Platform = AuthenticatorPlatform.Google;
        }
        else if (@this.Value is MicrosoftAuthenticator)
        {
            m.Platform = AuthenticatorPlatform.Microsoft;
        }

        m.SecretKey = @this.Value.SecretKey;

        if (@this.Value.Period != DEFAULT_PERIOD)
        {
            m.Period = @this.Value.Period;
        }

        m.CodeDigits = @this.Value.CodeDigits;
        m.Name = label;

        return m;
    }

    /// <summary>
    /// 创建密钥 Uri 格式兼容的 URL
    /// <para>https://code.google.com/p/google-authenticator/wiki/KeyUriFormat</para>
    /// </summary>
    /// <param name="this"></param>
    /// <param name="compat"></param>
    /// <returns></returns>
    public static string ToUrl(
        this AuthenticatorExportModel @this,
        bool compat = false)
    {
        string type = "totp";
        StringBuilder extraparams = new();

        var issuer = @this.Issuer;
        var label = @this.Name;
        if (!string.IsNullOrEmpty(issuer))
        {
            extraparams.Append("&issuer=");
            extraparams.Append(HttpUtility.UrlEncode(issuer));
        }

        if (@this.HMACType != DEFAULT_HMAC_TYPE)
        {
            extraparams.Append("&algorithm=");
            extraparams.Append(@this.HMACType.ToString());
        }

        if (@this.Platform == AuthenticatorPlatform.BattleNet)
        {
            extraparams.Append("&serial=");
            extraparams.Append(HttpUtility.UrlEncode(@this.Serial?.Replace("-", "")));
        }
        else if (@this.Platform == AuthenticatorPlatform.Steam)
        {
            if (!compat)
            {
                extraparams.Append("&deviceid=");
                extraparams.Append(HttpUtility.UrlEncode(@this.DeviceId));

                extraparams.Append("&data=");
                extraparams.Append(HttpUtility.UrlEncode(@this.SteamData));
            }
        }
        else if (@this.Platform == AuthenticatorPlatform.HOTP)
        {
            type = "hotp";
            extraparams.Append("&counter=");
            extraparams.Append(@this.Counter);
        }

        var secret = HttpUtility.UrlEncode(Base32.GetInstance().Encode(@this.SecretKey ?? Array.Empty<byte>()));

        if (@this.Period != DEFAULT_PERIOD)
        {
            extraparams.Append("&period=");
            extraparams.Append(@this.Period);
        }

        StringBuilder url = new("otpauth://");
        url.Append(type);
        url.Append('/');
        if (!string.IsNullOrEmpty(issuer))
        {
            url.Append(HttpUtility.UrlPathEncode(issuer));
            url.Append(':');
        }
        url.Append(HttpUtility.UrlPathEncode(label));
        url.Append("?secret=");
        url.Append(secret);
        url.Append("&digits=");
        url.Append(@this.CodeDigits);
        url.Append(extraparams);

        var url_ = url.ToString();
        return url_;
    }
}
