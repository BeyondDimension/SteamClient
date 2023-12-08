namespace BD.SteamClient8.UnitTest;

#pragma warning disable SA1600
#pragma warning disable NUnit1032 // An IDisposable field/property should be Disposed in a TearDown method

/// <summary>
/// <see cref="SteamTradeServiceImpl"/> 单元测试
/// </summary>
class SteamTradeServiceTest
{
    IServiceProvider service;

    SteamLoginState loginState = null;

    ISteamTradeService Trade => service.GetRequiredService<ISteamTradeService>();

    ISteamAccountService Client => service.GetRequiredService<ISteamAccountService>();

    [SetUp]
    public void SetUp()
    {
        var services = new ServiceCollection();
        services.TryAddHttpPlatformHelper();
        services.AddLogging();
        services.AddSteamAccountService();
        services.AddSteamIdleCardService();
        service = services.BuildServiceProvider();

        if (loginState == null)
        {
            string path = $"{AppDomain.CurrentDomain.BaseDirectory}/state.json";

            if (File.Exists(path))
            {
                using FileStream fs = new FileStream(path, FileMode.Open);

                loginState = SystemTextJsonSerializer.Deserialize<SteamLoginState>(fs);
            }
            else
            {
                var localPath = @"C:\Users\CYCY\Desktop\session.json";
                var json = JsonDocument.Parse(File.ReadAllText(localPath)).RootElement;
                loginState = new SteamLoginState()
                {
                    Username = json.GetProperty("userName").ToString(),
                    Password = json.GetProperty("passWord").ToString()
                };
                Client.DoLoginV2Async(loginState!).GetAwaiter().GetResult();
                Client.DoLoginV2Async(loginState!).GetAwaiter().GetResult();
                string x = SystemTextJsonSerializer.Serialize(loginState);
                File.WriteAllText(path, x);
            }
        }
    }
}
