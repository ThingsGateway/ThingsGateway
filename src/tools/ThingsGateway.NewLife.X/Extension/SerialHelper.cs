//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

using ThingsGateway.NewLife.X.Reflection;

namespace ThingsGateway.NewLife.X.Serialization;

/// <summary>序列化助手</summary>
public static class SerialHelper
{
    private static readonly ConcurrentDictionary<PropertyInfo, String> _cache = new();

    /// <summary>依据 Json/Xml 字典生成实体模型类</summary>
    /// <param name="dic"></param>
    /// <param name="className"></param>
    /// <returns></returns>
    public static String? BuildModelClass(this IDictionary<String, Object?> dic, String className = "Model")
    {
        if (dic == null || dic.Count == 0) return null;

        var sb = new StringBuilder();

        BuildModel(sb, dic, className, null);

        return sb.ToString();
    }

    /// <summary>获取序列化名称</summary>
    /// <param name="pi"></param>
    /// <returns></returns>
    public static String GetName(PropertyInfo pi)
    {
        if (_cache.TryGetValue(pi, out var name)) return name;

        if (name.IsNullOrEmpty())
        {
            var att = pi.GetCustomAttribute<DataMemberAttribute>();
            if (att != null && !att.Name.IsNullOrEmpty()) name = att.Name;
        }
        if (name.IsNullOrEmpty())
        {
            var att = pi.GetCustomAttribute<XmlElementAttribute>();
            if (att != null && !att.ElementName.IsNullOrEmpty()) name = att.ElementName;
        }
        if (name.IsNullOrEmpty()) name = pi.Name;

        _cache.TryAdd(pi, name);

        return name;
    }

    private static void BuildModel(StringBuilder sb, IDictionary<String, Object?> dic, String className, String? prefix)
    {
        sb.AppendLine($"{prefix}public class {className}");
        sb.AppendLine($"{prefix}{{");

        var line = 0;
        foreach (var item in dic)
        {
            var name = item.Key;
            if (Char.IsLower(name[0])) name = Char.ToUpper(name[0]) + name[1..];

            if (line++ > 0) sb.AppendLine();

            var type = item.Value?.GetType() ?? typeof(Object);
            if (type.GetTypeCode() != TypeCode.Object)
                sb.AppendLine($"{prefix}\tpublic {type.Name} {name} {{ get; set; }}");
            else if (item.Value is IDictionary<String, Object?> sub)
            {
                var subclassName = name + "Model";
                sb.AppendLine($"{prefix}\tpublic {subclassName} {name} {{ get; set; }}");
                sb.AppendLine();

                BuildModel(sb, sub, subclassName, prefix + "\t");
            }
            else if (item.Value is IList<Object> list)
            {
                var elmType = list.Count > 0 ? list[0].GetType() : type.GetElementTypeEx();
                sb.AppendLine($"{prefix}\tpublic {elmType?.Name}[] {name} {{ get; set; }}");
            }
        }

        sb.AppendLine($"{prefix}}}");
    }
}
