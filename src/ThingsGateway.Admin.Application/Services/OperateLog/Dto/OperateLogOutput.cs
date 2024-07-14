//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Admin.Application;

public class OperateLogIndexOutput
{
    public string Name { get; set; }
    public string? OpAccount { get; set; }
    public string OpBrowser { get; set; }
    public string? OpIp { get; set; }
    public DateTime OpTime { get; set; }
}

public class OperateLogDayStatisticsOutput
{
    /// <summary>
    /// 日期
    /// </summary>
    public string Date { get; set; }

    /// <summary>
    /// 异常次数
    /// </summary>
    public int ExceptionCount { get; set; }

    /// <summary>
    /// 登录次数
    /// </summary>
    public int LoginCount { get; set; }

    /// <summary>
    /// 登出次数
    /// </summary>
    public int LogoutCount { get; set; }

    /// <summary>
    /// 操作次数
    /// </summary>
    public int OperateCount { get; set; }
}
