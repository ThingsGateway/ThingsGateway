// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

using System.IO.Ports;

using ThingsGateway.NewLife;

namespace ThingsGateway.Foundation;

public interface IChannelOptions
{

    /// <summary>
    /// 通道类型
    /// </summary>
    ChannelTypeEnum ChannelType { get; set; }

    #region 以太网

    /// <summary>
    /// 远程ip
    /// </summary>
    string RemoteUrl { get; set; }

    /// <summary>
    /// 本地绑定ip，分号分隔，例如：192.168.1.1:502;192.168.1.2:502，表示绑定192.168.1.1:502和192.168.1.2:502
    /// </summary>
    string BindUrl { get; set; }

    #endregion

    #region 串口

    /// <summary>
    /// COM
    /// </summary>
    string PortName { get; set; }

    /// <summary>
    /// 波特率
    /// </summary>
    int BaudRate { get; set; }

    /// <summary>
    /// 数据位
    /// </summary>
    int DataBits { get; set; }

    /// <summary>
    /// 校验位
    /// </summary>
    Parity Parity { get; set; }

    /// <summary>
    /// 停止位
    /// </summary>
    StopBits StopBits { get; set; }

    /// <summary>
    /// DtrEnable
    /// </summary>
    bool DtrEnable { get; set; }

    /// <summary>
    /// RtsEnable
    /// </summary>
    bool RtsEnable { get; set; }


    #endregion
    /// <summary>
    /// 最大并发数量
    /// </summary>
    int MaxConcurrentCount { get; set; }

    /// <summary>
    /// 组包缓存时间
    /// </summary>
    int CacheTimeout { get; set; }

    /// <summary>
    /// 连接超时时间
    /// </summary>
    ushort ConnectTimeout { get; set; }

    /// <summary>
    /// 通道并发控制锁
    /// </summary>
    WaitLock WaitLock { get; }

    TouchSocketConfig Config { get; set; }
}