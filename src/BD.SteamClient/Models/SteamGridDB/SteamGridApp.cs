namespace BD.SteamClient.Primitives.Models.SteamGridDB;

public class SteamGridApp
{
    /// <summary>
    /// 
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// 
    /// </summary>
    public List<string> Types { get; set; } = new();

    /// <summary>
    /// 
    /// </summary>
    public bool Verified { get; set; }
}

public class SteamGridAppData
{
    /// <summary>
    /// 
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public SteamGridApp Data { get; set; } = new();

    /// <summary>
    /// 
    /// </summary>
    public List<string> Errors { get; set; } = new();
}
