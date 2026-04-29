//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Foundation.Common.Json.Extension;

using TouchSocket.Core;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;

namespace ThingsGateway.Gateway.Razor;

[GeneratorRpcProxy(GeneratorFlag = CodeGeneratorFlag.ExtensionAsync)]
internal interface ITcpServiceComponentRpc : IRpcServer
{
    [DmtpRpc]
    Task<bool> OnDeleteAsync(long deviceId, List<TcpSessionClientDto> tcpSessionClientDtos);
    [DmtpRpc]
    Task<QueryData<TcpSessionClientDto>> OnQueryAsync(long deviceId, QueryPageOptions options);
}
public partial class TcpServiceComponentRpc : SingletonRpcServer, IPluginRpcServer, ITcpServiceComponentRpc
{

    public async Task<bool> OnDeleteAsync(long deviceId, List<TcpSessionClientDto> tcpSessionClientDtos)
    {
        ITcpServiceChannel? TcpServiceChannel = GlobalData.TryGetDeviceRuntime(deviceId, out DeviceRuntime deviceRuntime) ? deviceRuntime.Driver?.Channel as ITcpServiceChannel : null;
        if (TcpServiceChannel == null) return false;

        foreach (var item in tcpSessionClientDtos)
        {
            await TcpServiceChannel.ClientDisposeAsync(item.Id);
        }
        return true;

    }

    public Task<QueryData<TcpSessionClientDto>> OnQueryAsync(long deviceId, QueryPageOptions options)
    {
        ITcpServiceChannel? TcpServiceChannel = GlobalData.TryGetDeviceRuntime(deviceId, out DeviceRuntime deviceRuntime) ? deviceRuntime.Driver?.Channel as ITcpServiceChannel : null;
        if (TcpServiceChannel != null)
        {
            var clients = TcpServiceChannel.Clients.ToList();
            var data = clients.AdaptListTcpSessionClientDto();
            for (int i = 0; i < clients.Count; i++)
            {
                var client = clients[i];
                var pluginInfos = client.PluginManager.Plugins.Select(a =>
                {
                    var data = new
                    {
                        Name = a.GetType().Name,
                        Dict = new Dictionary<string, string>()
                    };

                    var propertyInfos = a.GetType().GetProperties();
                    foreach (var item in propertyInfos)
                    {
                        var type = item.PropertyType;
                        if (type.IsPrimitive || type.IsEnum || type == TouchSocketCoreUtility.StringType)
                        {
                            data.Dict.Add(item.Name, item.GetValue(a)?.ToString());
                        }
                    }
                    return data;
                }).ToList();
                data[i].PluginInfos = pluginInfos.ToSystemTextJsonString();
            }

            var query = data.GetQueryData(options);

            return Task.FromResult(query);
        }
        else
        {
            return Task.FromResult(new QueryData<TcpSessionClientDto>());
        }
    }
}
