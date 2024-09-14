//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Plugin.Dlt645;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class Dlt645_2007MasterProperty : CollectPropertyBase
{
    /// <summary>
    /// 客户端连接滑动过期时间
    /// </summary>
    [DynamicProperty]
    public int CheckClearTime { get; set; } = 120;

    /// <summary>
    /// 心跳检测
    /// </summary>
    [DynamicProperty]
    public string HeartbeatHexString { get; set; } = "FFFF8080";

    /// <summary>
    /// 读写超时时间
    /// </summary>
    [DynamicProperty]
    public ushort Timeout { get; set; } = 3000;

    /// <summary>
    /// 连接超时时间
    /// </summary>
    [DynamicProperty]
    public ushort ConnectTimeout { get; set; } = 3000;

    /// <summary>
    /// 帧前时间ms
    /// </summary>
    [DynamicProperty]
    public int SendDelayTime { get; set; } = 0;

    /// <summary>
    /// 组包缓存超时ms
    /// </summary>
    [DynamicProperty]
    public int CacheTimeout { get; set; } = 1000;

    /// <summary>
    /// 默认解析顺序
    /// </summary>
    [DynamicProperty]
    public DataFormatEnum DataFormat { get; set; }

    /// <summary>
    /// 默认地址
    /// </summary>
    [DynamicProperty]
    public string Station { get; set; }

    /// <summary>
    /// 默认DtuId
    /// </summary>
    [DynamicProperty]
    public string? DtuId { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    [DynamicProperty]
    public string Password { get; set; }

    /// <summary>
    /// 操作员代码
    /// </summary>
    [DynamicProperty]
    public string OperCode { get; set; }

    /// <summary>
    /// 前导符报文头
    /// </summary>
    [DynamicProperty]
    public string FEHead { get; set; } = "FEFEFEFE";

    public override int ConcurrentCount { get; set; } = 1;
}
