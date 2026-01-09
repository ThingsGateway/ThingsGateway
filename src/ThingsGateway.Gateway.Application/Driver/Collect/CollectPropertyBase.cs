//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 插件配置项
/// <br></br>
/// 使用<see cref="DynamicPropertyAttribute"/> 标识所需的配置属性
/// </summary>
public abstract class CollectPropertyBase : DriverPropertyBase
{
    /// <summary>
    /// 最大并发数量
    /// </summary>
    public virtual int MaxConcurrentCount { get; set; } = 1;

    /// <summary>
    /// 离线后恢复运行的间隔时间
    /// </summary>
    public virtual int ReIntervalTime { get; set; } = 0;

    /// <summary>
    /// 失败重试次数，默认3
    /// </summary>
    public virtual int RetryCount { get; set; } = 3;

    /// <summary>
    /// 读写占空比
    /// </summary>
    [MinValue(1)]
    public virtual int DutyCycle { get; set; } = 3;


    /// <summary>
    /// 写优先，写入时强制读取消操作
    /// </summary>
    public virtual bool WritePriority { get; set; } = false;
}
public class CollectPropertyNone : CollectPropertyBase
{
}
/// <summary>
/// 插件配置项
/// <br></br>
/// 使用<see cref="DynamicPropertyAttribute"/> 标识所需的配置属性
/// </summary>
public abstract class CollectPropertyRetryBase : CollectPropertyBase
{
    /// <summary>
    /// 失败重试次数，默认3
    /// </summary>
    [DynamicProperty]
    public override int RetryCount { get; set; } = 3;

    [DynamicProperty(Remark = "n 次写入操作会执行一次读取")]
    [MinValue(1)]
    public override int DutyCycle { get; set; } = 3;

    [DynamicProperty(Remark = "写优先，写入时强制读取消操作")]
    public override bool WritePriority { get; set; } = false;
}