using Resx1 = BD.SteamClient8.Resources.Strings;

namespace BD.SteamClient8.UnitTest;

sealed class ResxTest
{
    [Test]
    public void Test()
    {
        var s1 = GetProperties(typeof(Resx1));
        Assert.That(s1, Is.Not.Empty);

        TestContext.WriteLine(s1.Length.ToString());
    }

    static PropertyInfo[] GetProperties(Type type)
    {
        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Static);
        props = props.Where(x => x.PropertyType == typeof(string)).ToArray();
        foreach (var item in props)
        {
            item.GetValue(null);
        }
        return props;
    }
}
