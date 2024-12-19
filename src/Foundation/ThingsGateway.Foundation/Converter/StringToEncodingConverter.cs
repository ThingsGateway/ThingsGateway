//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Text;

namespace ThingsGateway.Foundation;

/// <inheritdoc/>
public class StringToEncodingConverter : ISerializerFormatter<string, object>
{
    /// <inheritdoc/>
    public int Order { get; set; }

    /// <inheritdoc/>
    public bool TryDeserialize(object state, in string source, Type targetType, out object target)
    {
        try
        {
            target = Encoding.Default;
            if (targetType == typeof(Encoding))
            {
                target = Encoding.GetEncoding(source);
                return true;
            }
        }
        catch
        {
            target = default;
            return false;
        }
        return false;
    }

    /// <inheritdoc/>
    public bool TrySerialize(object state, in object target, out string source)
    {
        try
        {
            if (target?.GetType() == typeof(Encoding))
            {
                source = (target as Encoding).WebName;
                return true;
            }
            source = target.ToJsonString();
            return true;
        }
        catch (Exception)
        {
            source = null;
            return false;
        }
    }
}
