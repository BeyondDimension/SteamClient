namespace BD.SteamClient.Models.Idle;

public class UserIdleInfo
{
    /// <summary>
    /// 用户等级
    /// </summary>
    public ushort UserLevel { get; set; }

    /// <summary>
    /// 用户当前经验
    /// </summary>
    public int CurrentExp { get; set; }

    /// <summary>
    /// 升级所需经验
    /// </summary>
    public int UpExp { get; set; }
}
