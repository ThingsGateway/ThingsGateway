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

namespace ThingsGateway.Foundation;

/// <summary>
/// LocalizerUtil
/// </summary>
public static class LocalizerUtil
{
    /// <inheritdoc/>
    public static Func<Type, IStringLocalizer> getLocalizer;

    /// <inheritdoc/>
    public static Func<Type, IStringLocalizer> GetLocalizer
    {
        get
        {
            if (getLocalizer == null)
                return (a) => new JsonLocalizer(a, "Locales");
            return getLocalizer;
        }
        private set
        {
            getLocalizer = value;
        }
    }

    /// <inheritdoc/>
    public static void SetLocalizerFactory(Func<Type, IStringLocalizer> func)
    {
        GetLocalizer = func;
    }
}
