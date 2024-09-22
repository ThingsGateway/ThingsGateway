//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.Localization;

using Newtonsoft.Json.Linq;

using System.Collections.Concurrent;
using System.Globalization;
using System.Text;

namespace ThingsGateway.Foundation;

/// <summary>
/// json资源多语言
/// </summary>
public class JsonLocalizer : IStringLocalizer
{
    private readonly ConcurrentDictionary<string, JObject> _resources = new();
    private string _folderName;
    private Type _type;

    /// <inheritdoc/>
    public JsonLocalizer(Type resourceType, string folderName)
    {
        _type = resourceType;
        _folderName = folderName;
    }

    /// <inheritdoc/>
    public LocalizedString this[string name]
    {
        get
        {
            if (_resources.TryGetValue(CultureInfo.CurrentUICulture.Name, out var resource))
            {
                if (resource.TryGetValue(name, out JToken value))
                {
                    return new LocalizedString(name, value.ToString());
                }
            }
            else
            {
                if (Add(_type, _folderName))
                {
                    if (_resources.TryGetValue(CultureInfo.CurrentUICulture.Name, out var resource1))
                    {
                        if (resource1.TryGetValue(name, out JToken value))
                        {
                            return new LocalizedString(name, value.ToString());
                        }
                    }
                }
            }

            return new LocalizedString(name, name, true);
        }
    }

    /// <inheritdoc/>
    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            string value = this[name].Value;
            return new LocalizedString(name, string.Format(value, arguments));
        }
    }

    /// <inheritdoc/>
    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        if (_resources.TryGetValue(CultureInfo.CurrentUICulture.Name, out var resource))
        {
            foreach (var item in resource)
            {
                yield return new LocalizedString(item.Key, item.Value.ToString());
            }
        }
    }

    private bool Add(Type resourceType, string folderName)
    {
        try
        {
            var assembly = resourceType.Assembly;
            var culture = CultureInfo.CurrentUICulture.Name;
            using var stream = assembly.GetManifestResourceStream($@"{assembly.GetName().Name}.{folderName}.{culture}.json");
            using var reader = new StreamReader(stream, Encoding.UTF8);
            var jsonData = reader.ReadToEnd();

            var resource = (JObject.Parse(jsonData)[resourceType.FullName] as JObject)!;
            _resources.TryAdd(culture, resource);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
