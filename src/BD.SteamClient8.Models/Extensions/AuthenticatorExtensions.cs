using BD.SteamClient8.Enums.Authenticators;
using BD.SteamClient8.Models.WinAuth;
using BD.SteamClient8.Models.WinAuth.Abstractions;
using System.Extensions;
using System.Text;
using System.Web;
using static BD.SteamClient8.Models.WinAuth.AuthenticatorValueModel;

namespace BD.SteamClient8.Models.Extensions;

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
}
