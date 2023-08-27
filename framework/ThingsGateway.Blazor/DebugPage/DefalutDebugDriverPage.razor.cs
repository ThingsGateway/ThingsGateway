#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using Microsoft.AspNetCore.Components;

using ThingsGateway.Application;

using TouchSocket.Core;

namespace ThingsGateway.Blazor;

/// <summary>
/// <inheritdoc/>
/// </summary>
public enum ChannelEnum
{
    /// <inheritdoc/>
    None = 0,
    /// <inheritdoc/>
    TcpClientEx = 1,
    /// <inheritdoc/>
    SerialPort = 2,
    /// <inheritdoc/>
    UdpSession = 3,
    /// <inheritdoc/>
    TcpServer = 4,
}
/// <inheritdoc/>
public partial class DefalutDebugDriverPage : DriverDebugUIBase
{
    /// <summary>
    /// SerialSessionPage
    /// </summary>
    public SerialSessionPage SerialSessionPage;
    /// <summary>
    /// TcpClientPage
    /// </summary>
    public TcpClientPage TcpClientPage;
    /// <summary>
    /// TcpServerPage
    /// </summary>
    public TcpServerPage TcpServerPage;
    /// <summary>
    /// UdpSessionPage
    /// </summary>
    public UdpSessionPage UdpSessionPage;
    /// <summary>
    /// 选择，1-TCPCLIENT，2-串口，3-UDP，4-TCPServer
    /// </summary>
    [Parameter]
    public ChannelEnum Channel { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override ThingsGateway.Foundation.IReadWriteDevice Plc { get; set; }

    /// <summary>
    /// 模板
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; }
    /// <summary>
    /// 自定义模板
    /// </summary>
    [Parameter]
    public RenderFragment OtherContent { get; set; }
    /// <inheritdoc/>
    public override void Dispose()
    {
        Plc?.SafeDispose();
        TcpClientPage?.SafeDispose();
        SerialSessionPage?.SafeDispose();
        TcpServerPage?.SafeDispose();
        UdpSessionPage?.SafeDispose();
        base.Dispose();
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="firstRender"></param>
    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            if (TcpClientPage != null)
                TcpClientPage.LogAction = LogOut;
            if (SerialSessionPage != null)
                SerialSessionPage.LogAction = LogOut;
            if (TcpServerPage != null)
                TcpServerPage.LogAction = LogOut;
            if (UdpSessionPage != null)
                UdpSessionPage.LogAction = LogOut;
            //载入配置
            StateHasChanged();
        }

        base.OnAfterRender(firstRender);
    }
}