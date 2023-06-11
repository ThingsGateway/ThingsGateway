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

namespace ThingsGateway.Web.Foundation;

/// <inheritdoc/>
public class CPU
{
    /// <summary>
    /// 对象的简短描述（单行字符串）。
    /// </summary>
    [Description("简述")]
    public string Caption { get; set; } = string.Empty;

    /// <summary>
    /// 处理器的当前速度，以 MHz 为单位。
    /// </summary>
    [Description("当前速度")]
    public UInt32 CurrentClockSpeed { get; set; }

    /// <summary>
    /// 对象的描述。
    /// </summary>
    [Description("描述")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 2 级处理器高速缓存的大小。 2 级高速缓存是一个外部存储器区域，其访问时间比主 RAM 存储器更快。
    /// </summary>
    [Description("2级高速缓存")]
    public UInt32 L2CacheSize { get; set; }

    /// <summary>
    /// 3 级处理器高速缓存的大小。 3 级高速缓存是一个外部存储器区域，其访问时间比主 RAM 存储器更快。
    /// </summary>
    [Description("3级高速缓存")]
    public UInt32 L3CacheSize { get; set; }

    /// <summary>
    /// 处理器制造商的名称。
    /// </summary>
    [Description("制造商")]
    public string Manufacturer { get; set; } = string.Empty;

    /// <summary>
    /// 处理器的最大速度，以 MHz 为单位。
    /// </summary>
    [Description("最大速度")]
    public UInt32 MaxClockSpeed { get; set; }

    /// <summary>
    /// 已知对象的标签。
    /// </summary>
    [Description("名称")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 当前处理器实例的内核数。核心是集成电路上的物理处理器。例如，在双核处理器中，此属性的值为 2。
    /// </summary>
    [Description("物理处理器")]
    public UInt32 NumberOfCores { get; set; }

    /// <summary>
    /// 当前处理器实例的逻辑处理器数。对于具有超线程能力的处理器，该值仅包括启用了超线程的处理器。
    /// </summary>
    [Description("逻辑处理器")]
    public UInt32 NumberOfLogicalProcessors { get; set; }

    /// <summary>
    /// 如果为 True，则处理器支持用于虚拟化的地址转换扩展。
    /// </summary>
    [Description("支持虚拟化")]
    public Boolean SecondLevelAddressTranslationExtensions { get; set; }

    /// <summary>
    /// 电路上使用的芯片插座类型。
    /// </summary>
    [Description("插座类型")]
    public string SocketDesignation { get; set; } = string.Empty;

    /// <summary>
    /// 如果为 True，则固件已启用虚拟化扩展。
    /// </summary>
    [Description("启用虚拟化")]
    public Boolean VirtualizationFirmwareEnabled { get; set; }

    /// <summary>
    /// 如果为 True，则处理器支持 Intel 或 AMD 虚拟机监视器扩展。
    /// </summary>
    [Description("支持监视器扩展")]
    public Boolean VMMonitorModeExtensions { get; set; }

    /// <summary>
    /// % Processor Time 是处理器执行非空闲线程所用时间的百分比。
    /// 它是通过测量处理器执行空闲线程所花费的时间百分比，然后从 100% 中减去该值来计算的。
    /// （每个处理器都有一个空闲线程，当没有其他线程准备好运行时，它会消耗周期）。
    /// 此计数器是处理器活动的主要指标，并显示在采样间隔期间观察到的平均繁忙时间百分比。
    /// 需要注意的是，处理器是否空闲的计费计算是以系统时钟的内部采样间隔（10ms）进行的。
    /// 在当今的快速处理器上，% Processor Time 因此会低估处理器利用率，因为处理器可能会在系统时钟采样间隔之间花费大量时间来服务线程。
    /// 基于工作负载的定时器应用程序是更可能不准确测量的应用程序的一个示例，因为定时器在采样后立即发出信号。
    /// </summary>
    [Description("CPU总占用率")]
    public UInt64 PercentProcessorTime { get; set; }

    /// <inheritdoc/>
    public List<CpuCore> CpuCoreList { get; set; } = new List<CpuCore>();

}
