namespace BD.SteamClient8.WinAuth.Enums;

/// <summary>
/// 用于加密机密数据的密码类型
/// </summary>
public enum PasswordTypes
{
    None = 0,
    Explicit = 1,
    User = 2,
    Machine = 4,
    YubiKeySlot1 = 8,
    YubiKeySlot2 = 16,
}