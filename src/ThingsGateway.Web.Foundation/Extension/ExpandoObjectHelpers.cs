using System.Dynamic;
using System.Linq;
using System.Reflection;

using ThingsGateway.Core;
using ThingsGateway.Web.Foundation;
/// <summary>
/// 动态类型扩展
/// </summary>
public static class ExpandoObjectHelpers
{

    /// <summary>
    /// 动态类型转换，需使用特性<see cref="ExcelAttribute"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="expandoObject"></param>
    /// <returns></returns>
    public static T ConvertToEntity<T>(this ExpandoObject expandoObject) where T : new()
    {
        var entity = new T();
        var properties = typeof(T).GetAllProps().Where(a => a.GetCustomAttribute<ExcelAttribute>() != null);

        foreach (var keyValuePair in (IDictionary<string, object>)expandoObject)
        {
            var property = properties.FirstOrDefault(p => p.FindDisplayAttribute() == keyValuePair.Key);
            if (property != null)
            {
                var value = keyValuePair.Value;
                if (value != null)
                {
                    var objValue = ReadWriteHelpers.ObjToTypeValue(property, value.ToString());
                    property.SetValue(entity, objValue);
                }
                else
                {
                    property.SetValue(entity, null);

                }

            }
        }

        return entity;
    }
}