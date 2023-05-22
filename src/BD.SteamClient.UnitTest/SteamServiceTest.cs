namespace BD.SteamClient.UnitTest;

public sealed class SteamServiceTest
{
    IServiceProvider service;

    ISteamService Client => service.GetRequiredService<ISteamService>();

    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddLogging(l => l.AddProvider(NullLoggerProvider.Instance));
        services.AddSingleton<ISteamService, TestSteamServiceImpl>();
        service = services.BuildServiceProvider();
    }

    [Test]
    public void TestGetRememberUserList()
    {
        var list = Client.GetRememberUserList();
        list.ForEach(x =>
        {
            TestContext.WriteLine($"{x.SteamId64}   {x.SteamID}");
        });
        Assert.True(list.Any_Nullable());
    }

    [Test]
    public void TestGetDownloadingAppList()
    {
        var list = Client.GetDownloadingAppList();
        list.ForEach(x =>
        {
            TestContext.WriteLine($"{x.Name}   {x.AppId}");
        });
        Assert.True(list.Any_Nullable());
    }

    sealed class TestSteamServiceImpl : SteamServiceImpl
    {
        public TestSteamServiceImpl(ILoggerFactory loggerFactory) : base(loggerFactory)
        {

        }

        public override ISteamConnectService Conn => throw new NotImplementedException();

        protected override string? StratSteamDefaultParameter => default;

        protected override bool IsRunSteamAdministrator => default;

        protected override Dictionary<uint, string?>? HideGameList => default;
    }
}