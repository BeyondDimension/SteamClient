using System.Extensions;

namespace BD.SteamClient8.UnitTest;

#pragma warning disable IDE1006 // 命名样式
sealed class _ProjectUtilsTest
#pragma warning restore IDE1006 // 命名样式
{
    [Test]
    public void Test()
    {
        TestContext.WriteLine($"tfm: {tfm}");
        TestContext.WriteLine($"IsCI: {IsCI().ToLowerString()}");
        TestContext.WriteLine($"ProjPath: {ProjPath}");
        TestContext.WriteLine($"ROOT_ProjPath: {ROOT_ProjPath}");
        TestContext.WriteLine($"DataPath: {DataPath}");
    }
}
