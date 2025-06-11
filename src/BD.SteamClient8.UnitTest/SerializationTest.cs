using BD.SteamClient8.Models;
using DotNext.Collections.Generic;
using DotNext.Reflection;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Extensions;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using static BD.Common8.UnitTest.SerializationTestHelper;

namespace BD.SteamClient8.UnitTest;

/// <summary>
/// 模型类的序列化测试，JSON 源生成测试与 AOT 兼容
/// </summary>
sealed class SerializationTest
{
    const string modelAsmName = "BD.SteamClient8.Models";
    const string modelNamespaceStartsWith = "BD.SteamClient8.Models";
    const string serviceAsmName = "BD.SteamClient8";
    const string serviceNamespaceStartsWith = "BD.SteamClient8.Services";

    static JsonSerializerContext JSC => DefaultJsonSerializerContext_.Default;

    ImmutableArray<Type> modelTypes;

    static bool IsProtobufModelType(Type t)
    {
        return t.Namespace != null && t.Namespace.StartsWith("BD.SteamClient8.Models.Protobuf");
    }

    [SetUp]
    public void Setup()
    {
        var modelAsm = Assembly.Load(modelAsmName).ThrowIsNull();
        var q = GetModelTypesByModelAssemblies(modelNamespaceStartsWith,
            t =>
                IsProtobufModelType(t) == false // 排除 Protobuf 模型类
                && t != typeof(SteamIdConvert),
            modelAsm);
        modelTypes = [.. q];
        if (modelTypes.Length == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(modelTypes));
        }
    }

    /// <summary>
    /// 测试模型类程序集内的所有模型类型必须通过 JSON 序列化和反序列化测试
    /// </summary>
    /// <exception cref="AggregateException"></exception>
    [Test]
    public void JsonByModels()
    {
        List<Exception> exceptions = [];
        foreach (var t in modelTypes)
        {
            Json(JSC, t, exceptions, static x => x);
        }
        if (exceptions.Count != 0)
        {
            throw new AggregateException(null, exceptions);
        }
    }

    /// <summary>
    /// 测试服务接口上公开的参数与返回值中的模型类必须通过 JSON 序列化和反序列化测试
    /// </summary>
    [Test]
    public void JsonByServices()
    {
        var sAsm = Assembly.Load(serviceAsmName).ThrowIsNull();
        var modelTypeByServices = GetModelTypeDictByServiceAssemblies(serviceNamespaceStartsWith, null, sAsm);
        if (modelTypeByServices.Count == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(modelTypeByServices));
        }

        List<Exception> exceptions = [];
        foreach (var it in modelTypeByServices)
        {
            // BD.SteamClient8.Services 中类型不强制要求全部通过测试

            // 对于值类型元组，System.Text.Json 不支持，跳过测试
            if (IsValueTuple(it.Key) || IsValueTupleNullable(it.Key))
            {
                continue;
            }
            if (it.Key == typeof(CultureInfo))
            {
                continue;
            }
            if (it.Key.IsGenericType)
            {
                var gTypeDef = it.Key.GetGenericTypeDefinition();
                if (gTypeDef == typeof(IAsyncEnumerable<>))
                {
                    continue;
                }
            }
            if (IsProtobufModelType(it.Key))
            {
                continue; // Protobuf 模型类不需要进行 JSON 序列化测试
            }
            if (IsSimpleTypes(it.Key) || IsArraySimpleTypes(it.Key) || IsNullableSimpleTypes(it.Key))
            {
                continue;
            }
            if (typeof(Delegate).IsAssignableFrom(it.Key))
            {
                continue; // 跳过委托类型
            }

            // 白名单过滤
            if (modelTypes.Contains(it.Key))
            {
                continue; // 已经在模型类测试中通过
            }

            var isWhiteListType = false;
            if (it.Key.IsGenericType)
            {
                // 模型类作为泛型参数的类型，需要在 JsonSerializerContext 上标注 JsonSerializableAttribute
                isWhiteListType = it.Key.GenericTypeArguments.Any(x => modelTypes.Contains(x));
            }
            else if (it.Key.IsArray)
            {
                var ta = it.Key.GetTypeInfo().ImplementedInterfaces.Single(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>))!.GetGenericArguments()[0];
                if (ta.IsGenericType)
                {
                    isWhiteListType = ta.GenericTypeArguments.Any(x => modelTypes.Contains(x));
                }
                else if (modelTypes.Contains(ta))
                {
                    isWhiteListType = true; // 数组元素类型需要通过测试
                }
            }
            if (!isWhiteListType)
            {
                continue;
            }

            Json(JSC, it, exceptions, static x => x.Key, static (it, ex) =>
            {
                var errMsg =
$"""
测试接口 {it.Value} 的类型 {it.Key.FullName} 时发生异常。
    {ex}

""";
                return errMsg;
            });
        }
        if (exceptions.Count != 0)
        {
            throw new AggregateException(null, exceptions);
        }
    }
}
