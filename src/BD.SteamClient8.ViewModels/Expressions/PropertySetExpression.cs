namespace BD.SteamClient8.ViewModels.Expressions;

/// <summary>
/// 属性赋值表达式数
/// </summary>
public static class PropertySetExpression
{
    /// <summary>
    /// 相同属性名赋值
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TTarget"></typeparam>
    /// <returns></returns>
    public static Expression<Action<TSource, TTarget>> CreateCopyExpression<TSource, TTarget>()
    {
        var sourceParameter = Expression.Parameter(typeof(TSource), "source");
        var targetParameter = Expression.Parameter(typeof(TTarget), "target");

        var copyExpressions = typeof(TSource).GetProperties()
            .Where(prop => typeof(TTarget).GetProperty(prop.Name) != null)
            .Select(prop =>
            {
                var sourceProperty = Expression.Property(sourceParameter, prop);
                var targetProperty = Expression.Property(targetParameter, prop);

                return Expression.Assign(targetProperty, sourceProperty);
            });

        var block = Expression.Block(copyExpressions);

        return Expression.Lambda<Action<TSource, TTarget>>(block, sourceParameter, targetParameter);
    }
}
