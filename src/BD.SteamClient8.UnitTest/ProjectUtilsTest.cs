namespace BD.SteamClient8.UnitTest;

#pragma warning disable IDE1006 // 命名样式
#pragma warning disable SA1600 // Elements should be documented

sealed class _ProjectUtilsTest
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
