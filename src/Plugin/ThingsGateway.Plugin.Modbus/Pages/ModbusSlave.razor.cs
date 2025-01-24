//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

#pragma warning disable CA2007 // 考虑对等待的任务调用 ConfigureAwait
using BootstrapBlazor.Components;

using Microsoft.AspNetCore.Components;

using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Modbus;

using TouchSocket.Core;

namespace ThingsGateway.Debug;

public partial class ModbusSlave : ComponentBase, IDisposable
{
    private ThingsGateway.Foundation.Modbus.ModbusSlave _plc = new();

    private string LogPath;

    ~ModbusSlave()
    {
        this.SafeDispose();
    }

    private DeviceComponent DeviceComponent { get; set; }

    public void Dispose()
    {
        _plc?.SafeDispose();
        GC.SuppressFinalize(this);
    }
    private async Task OnShowAddressUI(string address)
    {
        var op = new DialogOption()
        {
            IsScrolling = false,
            ShowMaximizeButton = true,
            Size = Size.ExtraLarge,
            Title = nameof(ModbusAddress),
            ShowFooter = false,
            ShowCloseButton = false,
        };

        op.Component = BootstrapDynamicComponent.CreateComponent<ModbusAddressComponent>(new Dictionary<string, object?>
        {
             {nameof(ModbusAddressComponent.ModelChanged),  (string a) =>
            {
                if(DeviceComponent!=null)
                {
                    DeviceComponent.SetRegisterAddress(a);
                }
            }},
            {nameof(ModbusAddressComponent.Model),address },
        });

        await DialogService.Show(op);
    }
    [Inject]
    DialogService DialogService { get; set; }
    private void OnConfimClick((IChannel channel, string logPath) value)
    {
        _plc.InitChannel(value.channel);
        LogPath = value.logPath;
    }
}
