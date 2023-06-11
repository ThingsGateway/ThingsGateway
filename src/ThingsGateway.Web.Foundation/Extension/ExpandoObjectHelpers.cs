#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using System.Dynamic;
using System.Linq;
using System.Reflection;

using ThingsGateway.Core;
namespace ThingsGateway.Web.Foundation;
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
                var objValue = ReadWriteHelpers.ObjToTypeValue(property, value?.ToString() ?? "");
                property.SetValue(entity, objValue);

            }
        }

        return entity;
    }
}