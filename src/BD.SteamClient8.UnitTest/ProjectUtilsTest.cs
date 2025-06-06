using System.Extensions;

namespace BD.SteamClient8.UnitTest;

#pragma warning disable IDE1006 // 命名样式
sealed class _ProjectUtilsTest
#pragma warning restore IDE1006 // 命名样式
{
    [Test]
    public void Test()
    {
        TestContext.Out.WriteLine($"tfm: {tfm}");
        TestContext.Out.WriteLine($"IsCI: {IsCI().ToLowerString()}");
        TestContext.Out.WriteLine($"ProjPath: {ProjPath}");
        TestContext.Out.WriteLine($"ROOT_ProjPath: {ROOT_ProjPath}");
        TestContext.Out.WriteLine($"DataPath: {DataPath}");
    }
}
