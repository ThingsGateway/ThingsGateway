//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using ThingsGateway.NewLife;

namespace ThingsGateway.Admin.Application;

/// <inheritdoc/>
public class HardwareInfo
{
    /// <summary>
    /// 当前磁盘信息
    /// </summary>
    public DriveInfo DriveInfo { get; set; }

    /// <summary>
    /// 硬件信息获取
    /// </summary>
    public MachineInfo? MachineInfo { get; set; }

    /// <summary>
    /// 主机环境
    /// </summary>
    public string Environment { get; set; }

    /// <summary>
    /// NET框架
    /// </summary>
    public string FrameworkDescription { get; set; }

    /// <summary>
    /// 系统架构
    /// </summary>
    public string OsArchitecture { get; set; }

    /// <summary>
    /// 唯一编码
    /// </summary>
    public string UUID { get; set; }

    /// <summary>
    /// 进程占用内存
    /// </summary>
    [AutoGenerateColumn(Ignore = true)]
    public string WorkingSet { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public string UpdateTime { get; set; }
}
