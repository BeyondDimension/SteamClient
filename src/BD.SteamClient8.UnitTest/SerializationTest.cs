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
using WinRT;

namespace BD.SteamClient8.UnitTest;

/// <summary>
/// 模型类的序列化测试，JSON 源生成测试与 AOT 兼容
/// </summary>
sealed class SerializationTest
{
    const string jsonObjectString = "{}";
    ImmutableArray<Type> modelTypes;

    /// <summary>
    /// 根据模型类型创建实例
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    static object CreateInstance(Type t)
    {
        if (t.FullName == "System.Void")
        {
            return null!;
        }

        try
        {
            if (t.IsInterface)
            {
                if (t.IsGenericType)
                {
                    var gTypeDef = t.GetGenericTypeDefinition();
                    if (gTypeDef == typeof(IEnumerable<>) || gTypeDef == typeof(IList<>) || gTypeDef == typeof(ICollection<>) || gTypeDef == typeof(IReadOnlyCollection<>) || gTypeDef == typeof(IReadOnlyList<>))
                    {
                        return CreateInstance(typeof(List<>).MakeGenericType(t.GenericTypeArguments[0]));
                    }
                    else if (gTypeDef == typeof(IDictionary<,>) || gTypeDef == typeof(IReadOnlyDictionary<,>))
                    {
                        return CreateInstance(typeof(Dictionary<,>).MakeGenericType(t.GenericTypeArguments[0], t.GenericTypeArguments[1]));
                    }
                }
            }

            if (t.IsArray)
            {
                var ta = t.GetTypeInfo().ImplementedInterfaces.Single(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>))!.GetGenericArguments()[0];
                var array = typeof(Array).GetMethod(nameof(Array.Empty))!.MakeGenericMethod(ta).Invoke(null, null);
                return array!;
            }

            var obj = Activator.CreateInstance(t);
            return obj.ThrowIsNull();
        }
        catch (Exception ex)
        {
            try
            {
                // 先尝试使用源生成执行
                var obj = JsonSerializer.Deserialize(jsonObjectString, t, DefaultJsonSerializerContext_.Default);
                return obj.ThrowIsNull();
            }
            catch
            {
                try
                {
                    var obj = JsonSerializer.Deserialize(jsonObjectString, t);
                    return obj.ThrowIsNull();
                }
                catch
                {
                    // 忽略反序列化的异常，抛出调用构造函数的异常
                    throw ex;
                }
            }
        }
    }

    static bool IsSimpleTypes(Type t)
    {
        var typeCode = Type.GetTypeCode(t);
        switch (typeCode)
        {
            case TypeCode.Empty:
                break;
            case TypeCode.DBNull:
                break;
            case TypeCode.Boolean:
                break;
            case TypeCode.Char:
                break;
            case TypeCode.SByte:
                break;
            case TypeCode.Byte:
                break;
            case TypeCode.Int16:
                break;
            case TypeCode.UInt16:
                break;
            case TypeCode.Int32:
                break;
            case TypeCode.UInt32:
                break;
            case TypeCode.Int64:
                break;
            case TypeCode.UInt64:
                break;
            case TypeCode.Single:
                break;
            case TypeCode.Double:
                break;
            case TypeCode.Decimal:
                break;
            case TypeCode.DateTime:
                break;
            case TypeCode.String:
                break;
            default:
                return false;
        }
        return true;
    }

    static bool IsNullableSimpleTypes(Type t)
    {
        if (t.IsGenericType)
        {
            var gTypeDef = t.GetGenericTypeDefinition();
            if (gTypeDef == typeof(Nullable<>))
            {
                return IsSimpleTypes(t.GenericTypeArguments[0]);
            }
        }
        return false;
    }

    static bool IsArraySimpleTypes(Type t)
    {
        if (t.IsArray)
        {
            var ta = t.GetTypeInfo().ImplementedInterfaces.Single(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>))!.GetGenericArguments()[0];
            return IsSimpleTypes(ta);
        }
        return false;
    }

    static bool IsProtobufModelType(Type t)
    {
        return t.Namespace != null && t.Namespace.StartsWith("BD.SteamClient8.Models.Protobuf");
    }

    [SetUp]
    public void Setup()
    {
        var modelAsm = Assembly.Load("BD.SteamClient8.Models").ThrowIsNull();

        var tJsonSerializerContext = typeof(JsonSerializerContext);
        var tJsonConverter = typeof(JsonConverter);
        var q = from t in modelAsm.GetTypes()
                where t.Namespace != null && t.Namespace.StartsWith("BD.SteamClient8.Models") // 命名空间过滤
                    && t.FullName != null && t.FullName.Contains("+<") == false // 排除源生成的类型
                    && t.IsStatic() == false && t.IsAbstract == false && t.IsClass && !t.IsInterface // 排除静态类和抽象类与结构、接口
                    && t.IsSubclassOf(tJsonSerializerContext) == false // 排除 JsonSerializerContext 的子类
                    && t.IsSubclassOf(tJsonConverter) == false // 排除 JsonConverter 的子类
                    && IsProtobufModelType(t) == false // 排除 Protobuf 模型类
                    && t != typeof(SteamIdConvert)
                select t;
        modelTypes = [.. q];
        if (modelTypes.Length == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(modelTypes));
        }
    }

    static void Json<T>(T it, List<Exception> exceptions, Func<T, Type> getType, Func<T, Exception, string>? getErrMsg = null)
    {
        var t = getType(it);
        try
        {
            var obj = CreateInstance(t);
            Assert.That(obj, Is.Not.Null, $"创建 {t.FullName} 实例失败。");
            // 测试序列化
            var json = JsonSerializer.Serialize(obj, t, DefaultJsonSerializerContext_.Default);
            Assert.That(json, Is.Not.Null.Or.Empty, $"序列化 {t.FullName} 失败。");
            // 测试反序列化
            var deserializedObj = JsonSerializer.Deserialize(json, t, DefaultJsonSerializerContext_.Default);
            Assert.That(deserializedObj, Is.Not.Null, $"反序列化 {t.FullName} 失败。");
        }
        catch (Exception ex)
        {
            var errMsg = getErrMsg?.Invoke(it, ex) ??
$"""
测试类型 {t.FullName} 时发生异常。
    {ex}

""";
            exceptions.Add(new ApplicationException(errMsg));
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
            Json(t, exceptions, static x => x);
        }
        if (exceptions.Count != 0)
        {
            throw new AggregateException(null, exceptions);
        }
    }

    static bool IsModelType(Type t, [NotNullWhen(true)] out Type? modelType)
    {
        if (t == typeof(Task) || t == typeof(ValueTask) || t == typeof(CancellationToken))
        {
        }
        else
        {
            var typeCode = Type.GetTypeCode(t);
            switch (typeCode)
            {
                case TypeCode.Object:
                    {
                        if (t.IsGenericType)
                        {
                            var genericTypeDefinition = t.GetGenericTypeDefinition();
                            if (genericTypeDefinition == typeof(Task<>) || genericTypeDefinition == typeof(ValueTask<>))
                            {
                                return IsModelType(t.GetGenericArguments()[0], out modelType);
                            }
                        }
                        modelType = t;
                        return true;
                    }
            }
        }
        modelType = null;
        return false;
    }

    /// <summary>
    /// 判断类型是否为 ValueTuple
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    static bool IsValueTuple(Type t)
    {
        if (t.IsGenericType)
        {
            var genericTypeDefinition = t.GetGenericTypeDefinition();
            if (genericTypeDefinition.IsValueType)
            {
                if (typeof(ITuple).IsAssignableFrom(genericTypeDefinition))
                {
                    return true; // 是值元组类型
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 判断类型是否为 ValueTuple 的可空版本
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    static bool IsValueTupleNullable(Type t)
    {
        if (t.IsGenericType)
        {
            var genericTypeDefinition = t.GetGenericTypeDefinition();
            if (genericTypeDefinition == typeof(Nullable<>))
            {
                return IsValueTuple(t.GenericTypeArguments[0]);
            }
        }
        return false;
    }

    /// <summary>
    /// 测试服务接口上公开的参数与返回值中的模型类必须通过 JSON 序列化和反序列化测试
    /// </summary>
    [Test]
    public void JsonByServices()
    {
        var sAsm = Assembly.Load("BD.SteamClient8").ThrowIsNull();
        var q = from t in sAsm.GetTypes()
                where t.Namespace != null && t.Namespace.StartsWith("BD.SteamClient8.Services") // 命名空间过滤
                    && t.FullName != null && t.FullName.Contains("+<") == false // 排除源生成的类型
                    && t.IsInterface // 仅接口
                select t;
        Dictionary<Type, string> types = new();
        Type[] serviceTypes = [.. q];
        foreach (var serviceType in serviceTypes)
        {
            var properties = serviceType.GetProperties().Select(static x => x.PropertyType);
            foreach (var p in properties)
            {
                if (!serviceTypes.Contains(p) && IsModelType(p, out var modelType) && !types.ContainsKey(modelType))
                {
                    types.Add(modelType, $"{serviceType.FullName}.{p.Name}");
                }
            }
            var methods = serviceType.GetMethods();
            foreach (var m in methods)
            {
                var parameterTypes = m.GetParameterTypes();
                if (parameterTypes != null)
                {
                    foreach (var a in parameterTypes)
                    {
                        if (!serviceTypes.Contains(a) && IsModelType(a, out var modelType) && !types.ContainsKey(modelType))
                        {
                            types.Add(modelType, $"{serviceType.FullName}.{m.Name}");
                        }
                    }
                }
                var returnType = m.ReturnType;
                if (returnType != null && returnType.FullName != "System.Void")
                {
                    if (!serviceTypes.Contains(returnType) && IsModelType(returnType, out var modelType) && !types.ContainsKey(modelType))
                    {
                        types.Add(modelType, $"{serviceType.FullName}.{m.Name}");
                    }
                }
            }
        }

        List<Exception> exceptions = [];
        foreach (var it in types)
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

            Json(it, exceptions, static x => x.Key, static (it, ex) =>
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
