//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Gateway.Razor;

public interface IGatewayExportService
{
    Task<bool> OnChannelExport(List<Channel> data);
    Task<bool> OnDeviceExport(List<Device> data, string channelName, string plugin);
    Task<bool> OnVariableExport(List<Variable> data, string devName);
    Task<bool> OnChannelExport(GatewayExportFilter exportFilter);
    Task<bool> OnDeviceExport(GatewayExportFilter exportFilter);
    Task<bool> OnVariableExport(GatewayExportFilter exportFilter);
    Task<bool> OnMemoryVariableExport(List<MemoryVariable> data, string devName);
    Task<bool> OnMemoryVariableExport(GatewayExportFilter exportFilter);
}
