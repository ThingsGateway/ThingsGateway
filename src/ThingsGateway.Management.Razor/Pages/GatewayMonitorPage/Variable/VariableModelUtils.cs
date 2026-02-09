using System.Linq.Expressions;

using ThingsGateway.Common.Extension;
using ThingsGateway.Foundation.Common.Caching;
using ThingsGateway.Foundation.Common.Json.Extension;
namespace ThingsGateway.Gateway.Razor;

public static class VariableModelUtils
{
    static MemoryCache MemoryCache = new();
    private static object GetPropertyValue(VariableRuntime model, string fieldName)
    {
        if (model == null)
        {
            throw new ArgumentNullException(nameof(model));
        }
        var type = model.GetType();
        var key = $"{type.TypeHandle.Value}{fieldName}";
        if (MemoryCache.TryGetValue(key, out Func<object, object?> data))
        {
            return data(model);
        }
        else
        {
            var ret = MemoryCache.GetOrAdd(key, (key) =>
        {
            return CreatePropertyGetter(type, fieldName);
        })(model);
            return ret;
        }
    }

    /// <summary>
    /// 获取属性方法 Lambda 表达式
    /// </summary>
    private static Func<object, object?> CreatePropertyGetter(Type modelType, string propertyPath)
    {
        var parameter = Expression.Parameter(typeof(object), "obj");
        Expression current = Expression.Convert(parameter, modelType);

        foreach (var name in propertyPath.Split('.'))
        {
            var prop = modelType.GetPropertyByName(name)
                ?? throw new InvalidOperationException(
                    $"类型 {modelType.Name} 未找到属性 {name}");

            current = Expression.Property(current, prop);
            modelType = prop.PropertyType;
        }

        // box
        var body = Expression.Convert(current, typeof(object));
        return Expression.Lambda<Func<object, object?>>(body, parameter).Compile();
    }


    public static string GetValue(VariableRuntime row, string fieldName)
    {
        try
        {

            switch (fieldName)
            {
                case nameof(VariableRuntime.Value):
                    return row.Value?.ToSystemTextJsonString(false) ?? string.Empty;
                case nameof(VariableRuntime.RawValue):
                    return row.RawValue?.ToSystemTextJsonString(false) ?? string.Empty;
                case nameof(VariableRuntime.LastSetValue):
                    return row.LastSetValue?.ToSystemTextJsonString(false) ?? string.Empty;
                case nameof(VariableRuntime.ChangeTime):
                    return row.ChangeTime.ToString("MM-dd HH:mm:ss.fff");

                case nameof(VariableRuntime.CollectTime):
                    return row.CollectTime.ToString("MM-dd HH:mm:ss.fff");

                case nameof(VariableRuntime.IsOnline):
                    return row.IsOnline ? "Online" : "Offline";

                case nameof(VariableRuntime.LastErrorMessage):
                    return row.LastErrorMessage;


                case nameof(VariableRuntime.RuntimeType):
                    return row.RuntimeType;
                default:

                    var ret = VariableModelUtils.GetPropertyValue(row, fieldName);

                    if (ret != null)
                    {
                        var t = ret.GetType();
                        if (t.IsEnum)
                        {
                            // 如果是枚举这里返回 枚举的描述信息
                            var itemName = ret.ToString();
                            if (!string.IsNullOrEmpty(itemName))
                            {
                                ret = Utility.GetDisplayName(t, itemName);
                            }
                        }
                    }
                    return ret is string str ? str : ret?.ToString() ?? string.Empty;
            }


        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    internal static Alignment GetAlign(this ITableColumn col) => col.Align ?? Alignment.None;
    internal static bool GetTextWrap(this ITableColumn col) => col.TextWrap ?? false;
    internal static bool GetShowTips(this ITableColumn col) => col.ShowTips ?? false;

    internal static bool GetTextEllipsis(this ITableColumn col) => col.TextEllipsis ?? false;
}
