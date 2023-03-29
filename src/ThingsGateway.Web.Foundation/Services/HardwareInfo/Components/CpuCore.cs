namespace ThingsGateway.Web.Foundation
{
    /// <inheritdoc/>
    public class CpuCore
    {
        /// <summary>
        /// 已知对象的标签。
        /// </summary>
        [Description("名称")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// % Processor Time 是处理器执行非空闲线程所用时间的百分比。
        /// 它是通过测量处理器执行空闲线程所花费的时间百分比，然后从 100% 中减去该值来计算的。
        /// （每个处理器都有一个空闲线程，当没有其他线程准备好运行时，它会消耗周期）。
        /// 此计数器是处理器活动的主要指标，并显示在采样间隔期间观察到的平均繁忙时间百分比。
        /// 需要注意的是，处理器是否空闲的计费计算是以系统时钟的内部采样间隔（10ms）进行的。
        /// 在当今的快速处理器上，% Processor Time 因此会低估处理器利用率，因为处理器可能会在系统时钟采样间隔之间花费大量时间来服务线程。
        /// 基于工作负载的定时器应用程序是更可能不准确测量的应用程序的一个示例，因为定时器在采样后立即发出信号。
        /// </summary>
        [Description("CPU占用")]
        public UInt64 PercentProcessorTime { get; set; }

    }
}
