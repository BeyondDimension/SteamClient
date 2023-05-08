using BD.SteamClient.Services;
using BD.SteamClient.Services.Implementation;
using BD.SteamClient.Services.Mvvm;
using Microsoft.Extensions.Logging.Abstractions;

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

    sealed class TestSteamServiceImpl : SteamServiceImpl
    {
        public TestSteamServiceImpl(ILoggerFactory loggerFactory) : base(loggerFactory)
        {

        }

        public override ISteamConnectService Conn => throw new NotImplementedException();

        protected override string AppResources_SaveEditedAppInfo_SaveFailed => throw new NotImplementedException();

        protected override string? SteamSettings_StratParameter => throw new NotImplementedException();

        protected override bool SteamSettings_IsRunSteamAdministrator => throw new NotImplementedException();

        protected override Dictionary<uint, string?> GameLibrarySettings_HideGameList => throw new NotImplementedException();
    }
}