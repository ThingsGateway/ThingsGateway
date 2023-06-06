#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

namespace ThingsGateway.Web.Foundation;

/// <inheritdoc/>
public class MemoryStatus
{
    /// <summary>
    /// 当前进程可以提交的最大内存量，以字节为单位。
    /// 此值等于或小于系统范围的可用提交值。
    /// </summary>
    [Description("可提交内存")]
    public ulong AvailablePageFile { get; set; }

    /// <summary>
    /// 当前可用的物理内存量，以字节为单位。
    /// 这是无需先将其内容写入磁盘即可立即重用的物理内存量。
    /// 它是备用、空闲和零列表大小的总和。
    /// </summary>
    [Description("可用")]
    public ulong AvailablePhysical { get; set; }

    /// <summary>
    /// 当前在调用进程的虚拟地址空间的用户模式部分中未保留和未提交的内存量，以字节为单位。
    /// </summary>
    [Description("虚拟可提交内存")]
    public ulong AvailableVirtual { get; set; }

    /// <summary>
    /// 系统或当前进程的当前已提交内存限制，以字节为单位，以较小者为准。
    /// </summary>
    [Description("已提交内存")]
    public ulong TotalPageFile { get; set; }

    /// <summary>
    /// 实际物理内存量，以字节为单位。
    /// </summary>
    [Description("总")]
    public ulong TotalPhysical { get; set; }
    /// <summary>
    ///调用进程的虚拟地址空间的用户模式部分的大小，以字节为单位。
    /// 这个值取决于进程的类型、处理器的类型和操作系统的配置。
    /// </summary>
    [Description("虚拟已提交内存")]
    public ulong TotalVirtual { get; set; }
}
