#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Models;

public partial class AuthenticatorValueDTO : IAuthenticatorValueDTO
{
    /// <summary>
    /// Construct
    /// </summary>
    public AuthenticatorValueDTO()
    {
    }

    /// <summary>
    /// 判断 <see cref="AuthenticatorValueDTO"/> 实例是否具有值
    /// </summary>
    protected virtual bool ExplicitHasValue()
    {
        return SecretKey != null && CodeDigits > 0 && HMACType.IsDefined() && Period > 0;
    }

    /// <summary>
    /// 判断 <see cref="IExplicitHasValue"/> 实例是否具有值
    /// </summary>
    bool IExplicitHasValue.ExplicitHasValue() => ExplicitHasValue();
}