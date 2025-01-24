//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.ConfigurableOptions;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 历史硬件信息
/// </summary>
public class HardwareInfoOptions : IConfigurableOptions
{
    /// <summary>
    /// 启用
    /// </summary>
    public bool Enable { get; set; } = true;

    /// <summary>
    /// 历史保存间隔
    /// </summary>
    public int HistoryInterval { get; set; } = 60000;

    /// <summary>
    /// 历史保留天数
    /// </summary>
    public int DaysAgo { get; set; } = 7;
}
