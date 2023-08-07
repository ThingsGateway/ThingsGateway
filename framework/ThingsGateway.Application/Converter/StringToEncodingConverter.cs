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

using System.Text;

using TouchSocket.Core;

namespace ThingsGateway.Application;

/// <summary>
/// Json字符串转到对应类
/// </summary>
public class StringToEncodingConverter : IConverter<string>
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="source"></param>
    /// <param name="targetType"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public bool TryConvertFrom(string source, Type targetType, out object target)
    {
        try
        {
            target = Encoding.Default;
            if (targetType == typeof(Encoding))
            {
                if (source.Trim().ToUpper() == "UTF8")
                {
                    target = Encoding.UTF8;
                    return true;
                }
                else if (source.Trim().ToUpper() == "ASCII")
                {
                    target = Encoding.ASCII;
                    return true;
                }
                else if (source.Trim().ToUpper() == "UNICODE")
                {
                    target = Encoding.Unicode;
                    return true;
                }
                else if (source.Trim().ToUpper() == "DEFAULT")
                {
                    target = Encoding.Default;
                    return true;
                }
            }
        }
        catch
        {
            target = default;
            return false;
        }
        return false;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="target"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    public bool TryConvertTo(object target, out string source)
    {
        try
        {
            source = target.ToJson();
            return true;
        }
        catch (Exception)
        {
            source = null;
            return false;
        }
    }
}