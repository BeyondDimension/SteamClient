namespace BD.SteamClient8.UnitTest;

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
