//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Mapster;

using ThingsGateway.Admin.Application;
using ThingsGateway.Foundation;
using ThingsGateway.Gateway.Application;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Razor;

/// <summary>
/// HisAlarmPage
/// </summary>
public partial class TcpServicePage : IDriverUIBase
{
    [Parameter, EditorRequired]
    public object Driver { get; set; }

    [Inject]
    private ToastService ToastService { get; set; }

    public TcpSessionClientDto SearchModel { get; set; } = new TcpSessionClientDto();

    private async Task<bool> OnDeleteAsync(IEnumerable<TcpSessionClientDto> tcpSessionClientDtos)
    {
        try
        {
            foreach (var item in tcpSessionClientDtos)
            {
                await TcpServiceChannel.ClientDispose(item.Id);
            }
            return true;
        }
        catch (Exception ex)
        {
            await ToastService.Warning(null, $"{ex.Message}");
            return false;
        }
    }

    private Task<QueryData<TcpSessionClientDto>> OnQueryAsync(QueryPageOptions options)
    {
        if (TcpServiceChannel != null)
        {
            var query = TcpServiceChannel.Clients.ToList().Adapt<List<TcpSessionClientDto>>().GetQueryData(options);
            return Task.FromResult(query);
        }
        else
        {
            return Task.FromResult(new QueryData<TcpSessionClientDto>());
        }
    }

    public TcpServiceChannel? TcpServiceChannel => ((TcpServiceChannel)((DriverBase)Driver)?.Protocol?.Channel);
}

public class TcpSessionClientDto
{
    public string Id { get; set; }
    public string IP { get; set; }
    public int Port { get; set; }
    public DateTime LastReceivedTime { get; set; }
    public DateTime LastSentTime { get; set; }
}
