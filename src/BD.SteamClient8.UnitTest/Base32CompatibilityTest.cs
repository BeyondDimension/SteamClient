using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoogleBase32 = WinAuth.Base32; // 旧版 Base32 实现
using NetBase32 = BD.SteamClient8.WinAuth.Helpers.Base32;

namespace BD.SteamClient8.UnitTest;

/// <summary>
/// 测试旧版和新版Base32实现的兼容性
/// </summary>
[TestFixture]
sealed class Base32CompatibilityTest
{
    [Test]
    public void EmptyInput_BothImplementations_ShouldReturnEmptyString()
    {
        // 准备空数据
        byte[] emptyData = Array.Empty<byte>();

        // 测试编码
        string oldResult = GoogleBase32.GetInstance().Encode(emptyData);
        string newResult = NetBase32.ToBase32(emptyData);

        Assert.That(newResult, Is.EqualTo(oldResult));
        Assert.That(oldResult, Is.EqualTo(string.Empty));
        Assert.That(newResult, Is.EqualTo(string.Empty));

        // 测试解码
        byte[] oldDecoded = GoogleBase32.GetInstance().Decode("");
        byte[] newDecoded = NetBase32.FromBase32("");

        Assert.That(newDecoded, Is.EqualTo(oldDecoded));
        Assert.That(oldDecoded, Is.Empty);
        Assert.That(newDecoded, Is.Empty);
    }

    [TestCase("Hello World")]
    [TestCase("12345")]
    [TestCase("!@#$%^&*()")]
    [TestCase("测试中文")]
    public void NormalStrings_BothImplementations_ShouldReturnSameResult(string input)
    {
        // 准备数据
        byte[] data = Encoding.UTF8.GetBytes(input);

        // 测试编码
        string oldEncoded = GoogleBase32.GetInstance().Encode(data);
        string newEncoded = NetBase32.ToBase32(data);

        Assert.That(newEncoded, Is.EqualTo(oldEncoded), $"编码 '{input}' 结果不一致");

        // 测试解码
        byte[] oldDecoded = GoogleBase32.GetInstance().Decode(oldEncoded);
        byte[] newDecoded = NetBase32.FromBase32(newEncoded);

        Assert.That(newDecoded, Is.EqualTo(oldDecoded), "解码结果不一致");

        string oldDecodedString = Encoding.UTF8.GetString(oldDecoded);
        string newDecodedString = Encoding.UTF8.GetString(newDecoded);

        Assert.That(oldDecodedString, Is.EqualTo(input), "旧版解码后与原始输入不匹配");
        Assert.That(newDecodedString, Is.EqualTo(input), "新版解码后与原始输入不匹配");
    }

    [Test]
    public void BinaryData_BothImplementations_ShouldReturnSameResult()
    {
        // 准备随机二进制数据
        var random = new Random(42); // 固定种子以确保结果可重现
        byte[] data = new byte[100];
        random.NextBytes(data);

        // 测试编码
        string oldEncoded = GoogleBase32.GetInstance().Encode(data);
        string newEncoded = NetBase32.ToBase32(data);

        Assert.That(newEncoded, Is.EqualTo(oldEncoded), "随机二进制数据编码结果不一致");

        // 测试解码
        byte[] oldDecoded = GoogleBase32.GetInstance().Decode(oldEncoded);
        byte[] newDecoded = NetBase32.FromBase32(newEncoded);

        Assert.That(newDecoded, Is.EqualTo(oldDecoded), "随机二进制数据解码结果不一致");
        Assert.That(oldDecoded, Is.EqualTo(data), "旧版解码后与原始二进制数据不匹配");
        Assert.That(newDecoded, Is.EqualTo(data), "新版解码后与原始二进制数据不匹配");
    }

    [TestCase("AB CD-EF")]
    [TestCase("A-B-C-D-E")]
    [TestCase("A B C D E")]
    public void InputWithSeparators_BothImplementations_ShouldReturnSameResult(string encodedWithSeparators)
    {
        try
        {
            // 测试解码带有分隔符的输入
            byte[] oldDecoded = GoogleBase32.GetInstance().Decode(encodedWithSeparators);
            byte[] newDecoded = NetBase32.FromBase32(encodedWithSeparators);

            Assert.That(newDecoded, Is.EqualTo(oldDecoded), "带分隔符输入解码结果不一致");
        }
        catch (Exception ex)
        {
            // 如果两个实现都抛出异常，也视为一致
            Assert.Fail($"解码带分隔符的输入时出错: {ex.Message}");
        }
    }

    [Test]
    public void InputWithPadding_BothImplementations_ShouldReturnSameResult()
    {
        // 测试带填充的输入
        string encodedWithPadding = "MZXW6===";

        byte[] oldDecoded = GoogleBase32.GetInstance().Decode(encodedWithPadding);
        byte[] newDecoded = NetBase32.FromBase32(encodedWithPadding);

        Assert.That(newDecoded, Is.EqualTo(oldDecoded), "带填充字符的输入解码结果不一致");
    }

    [Test]
    public void DifferentCasing_BothImplementations_ShouldReturnSameResult()
    {
        // 测试不同大小写的输入
        string upperCase = "MZXW6YTBOI";
        string lowerCase = "mzxw6ytboi";

        byte[] oldDecodedUpper = GoogleBase32.GetInstance().Decode(upperCase);
        byte[] oldDecodedLower = GoogleBase32.GetInstance().Decode(lowerCase);
        byte[] newDecodedUpper = NetBase32.FromBase32(upperCase);
        byte[] newDecodedLower = NetBase32.FromBase32(lowerCase);

        Assert.That(oldDecodedLower, Is.EqualTo(oldDecodedUpper), "旧版对大小写敏感度不一致");
        Assert.That(newDecodedLower, Is.EqualTo(newDecodedUpper), "新版对大小写敏感度不一致");
        Assert.That(newDecodedUpper, Is.EqualTo(oldDecodedUpper), "新旧版对大写输入解码结果不一致");
        Assert.That(newDecodedLower, Is.EqualTo(oldDecodedLower), "新旧版对小写输入解码结果不一致");
    }
}