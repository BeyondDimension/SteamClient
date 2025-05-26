using BD.SteamClient8.WinAuth.Enums;
using BD.SteamClient8.WinAuth.Helpers;
using BD.SteamClient8.WinAuth.Models;
using System.Text;
using System.Web;
using static BD.SteamClient8.WinAuth.Models.AuthenticatorExportModel;

namespace BD.SteamClient8.WinAuth.Extensions;

public static partial class AuthenticatorExtensions
{
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

        var secret = HttpUtility.UrlEncode(Base32.ToBase32(@this.SecretKey));

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
