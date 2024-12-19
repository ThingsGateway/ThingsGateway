//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Foundation.Extension.String;

namespace ThingsGateway.Foundation;

/// <summary>
/// String值转换为基础类型。
/// </summary>
public class StringToClassConverter<TState> : ISerializerFormatter<string, TState>
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public int Order { get; set; } = -100;

    /// <inheritdoc/>
    public bool TryDeserialize(TState state, in string source, Type targetType, out object target)
    {
        return targetType.GetTypeValue(source, out target!);
    }

    /// <inheritdoc/>
    public bool TrySerialize(TState state, in object target, out string source)
    {
        if (target != null)
        {
            var targetType = target.GetType();
            return targetType.GetTypeStringValue(target, out source!);
        }
        else
        {
            source = null;
            return true;
        }
    }
}
