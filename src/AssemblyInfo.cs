namespace BD.SteamClient8;

/// <summary>
/// 提供用来获取应用程序信息（如版本号、说明、加载的程序集等）的属性。
/// </summary>
public static partial class AssemblyInfo
{
    /// <summary>
    /// 提供程序集的说明。
    /// <para><see cref="AssemblyTitleAttribute"/></para>
    /// </summary>
    public const string Title =
#if DEBUG
        $"[Debug] {Trademark}";
#else
        Trademark;
#endif

    /// <summary>
    /// 与应用程序关联的产品名称。
    /// <para><see cref="AssemblyTrademarkAttribute"/></para>
    /// </summary>
    public const string Trademark = "BD.SteamClient8";

    /// <summary>
    /// 与应用程序关联的产品名称。
    /// <para><see cref="AssemblyProductAttribute"/></para>
    /// </summary>
    public const string Product = Trademark;

    /// <summary>
    /// 提供程序集的文本说明。
    /// <para><see cref="AssemblyDescriptionAttribute"/></para>
    /// </summary>
    public const string Description = "Steam 相关服务类库"; // 不可更改

    /// <summary>
    /// 与该应用程序关联的公司名称。
    /// <para><see cref="AssemblyCompanyAttribute"/></para>
    /// </summary>
    public const string Company = "江苏蒸汽凡星科技有限公司"; // 不可更改

    /// <summary>
    /// 与应用程序关联的版权声明。
    /// <para><see cref="AssemblyCopyrightAttribute"/></para>
    /// </summary>
    public const string Copyright = $"©️ {Company}. All rights reserved."; // 不可更改

    /// <summary>
    /// 简体中文的区域性名称。
    /// </summary>
    public const string CultureName_SimplifiedChinese = "zh-Hans";
}
