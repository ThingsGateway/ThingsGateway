//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

global using BootstrapBlazor.Components;

global using Microsoft.AspNetCore.Components;
global using Microsoft.Extensions.Localization;

global using System.Diagnostics.CodeAnalysis;

global using ThingsGateway.Common;
global using ThingsGateway.Foundation;
global using ThingsGateway.Foundation.Common;
global using ThingsGateway.Foundation.Common.Extension;
global using ThingsGateway.Razor;
global using ThingsGateway.Management.Application;
global using ThingsGateway.Gateway.Application;

#if !Management
global using ChannelRuntime = ThingsGateway.Gateway.Application.ChannelRuntime;
global using DeviceRuntime = ThingsGateway.Gateway.Application.DeviceRuntime;
global using VariableRuntime = ThingsGateway.Gateway.Application.VariableRuntime;
global using MemoryVariableRuntime = ThingsGateway.Gateway.Application.MemoryVariableRuntime;
global using IVariablePageService = ThingsGateway.Gateway.Application.IVariablePageService;
global using IDevicePageService = ThingsGateway.Gateway.Application.IDevicePageService;
global using IChannelPageService = ThingsGateway.Gateway.Application.IChannelPageService;
global using IMemoryVariablePageService = ThingsGateway.Gateway.Application.IMemoryVariablePageService;
#else
global using ChannelRuntime = ThingsGateway.Management.Application.ChannelRuntime;
global using DeviceRuntime = ThingsGateway.Management.Application.DeviceRuntime;
global using VariableRuntime = ThingsGateway.Management.Application.VariableRuntime;
global using MemoryVariableRuntime = ThingsGateway.Management.Application.MemoryVariableRuntime;
global using IVariablePageService = ThingsGateway.Management.Application.IVariablePageService;
global using IDevicePageService = ThingsGateway.Management.Application.IDevicePageService;
global using IChannelPageService = ThingsGateway.Management.Application.IChannelPageService;
global using IMemoryVariablePageService = ThingsGateway.Management.Application.IMemoryVariablePageService;
#endif

[assembly: SuppressMessage("Reliability", "CA2007", Justification = "<挂起>", Scope = "module")]

[assembly: GlobalGenerateSetParametersAsync(true)]