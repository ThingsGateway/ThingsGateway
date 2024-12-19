﻿// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

using Microsoft.Extensions.Hosting;

namespace ThingsGateway.Gateway.Application;

public interface IBusinessDeviceHostedService : IDeviceHostedService, IHostedService
{
    /// <summary>
    /// 重启
    /// </summary>
    /// <param name="removeDevice">是否重新获取设备</param>
    /// <returns></returns>
    Task RestartAsync(bool removeDevice = true);

    /// <summary>
    /// 启用业务
    /// </summary>
    bool StartBusinessDeviceEnable { get; set; }

    /// <summary>
    /// 停止
    /// </summary>
    /// <param name="removeDevice">是否移除设备</param>
    /// <returns></returns>
    Task StopAsync(bool removeDevice);
}
