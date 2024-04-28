﻿
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------




using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Gateway.Razor;

public partial class IndexComponent : IDisposable
{
    [Parameter]
    [EditorRequired]
    public IStringLocalizer Localizer { get; set; }

    private Chart CollectDevicePie { get; set; }
    private Chart BusinessDevicePie { get; set; }
    private Chart VariablePie { get; set; }
    private Chart AlarmPie { get; set; }
    private bool CollectDevicePieInit { get; set; }
    private bool BusinessDevicePieInit { get; set; }
    private bool VariablePieInit { get; set; }
    private bool AlarmPieInit { get; set; }

    protected override void OnInitialized()
    {
        _ = RunTimerAsync();
        base.OnInitialized();
    }

    public bool Disposed { get; set; }

    public void Dispose()
    {
        Disposed = true;
        GC.SuppressFinalize(this);
    }

    private async Task RunTimerAsync()
    {
        while (!Disposed)
        {
            try
            {
                if (CollectDevicePieInit)
                    await CollectDevicePie.Update(ChartAction.Update);
                if (BusinessDevicePieInit)
                    await BusinessDevicePie.Update(ChartAction.Update);
                if (VariablePieInit)
                    await VariablePie.Update(ChartAction.Update);
                if (AlarmPieInit)
                    await AlarmPie.Update(ChartAction.Update);
                await InvokeAsync(StateHasChanged);
                await Task.Delay(5000);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex);
            }
        }
    }

    #region 曲线

    private ChartDataSource? CollectDeviceChartDataSource;
    private ChartDataSource? BusinessDeviceChartDataSource;
    private ChartDataSource? VariableChartDataSource;
    private ChartDataSource? AlarmChartDataSource;

    private Task<ChartDataSource> OnInitCollectDevicePie()
    {
        var data = typeof(DeviceStatusEnum).ToSelectList();
        if (CollectDeviceChartDataSource == null)
        {
            CollectDeviceChartDataSource = new ChartDataSource();
            CollectDeviceChartDataSource.Options.Title = Localizer["CollectDevice"];
            CollectDeviceChartDataSource.Options.ShowLegend = false;
            CollectDeviceChartDataSource.Labels = data.Select(a => a.Text);
            CollectDeviceChartDataSource.Data.Add(new ChartDataset()
            {
                Data = data.Select(i => GlobalData.ReadOnlyCollectDevices.Count(device => device.Value.DeviceStatus == (DeviceStatusEnum)Enum.Parse(typeof(DeviceStatusEnum), i.Value))).Cast<object>()
            });
        }
        else
        {
            CollectDeviceChartDataSource.Data[0].Data = data.Select(i => GlobalData.ReadOnlyCollectDevices.Count(device => device.Value.DeviceStatus == (DeviceStatusEnum)Enum.Parse(typeof(DeviceStatusEnum), i.Value))).Cast<object>();
        }
        return Task.FromResult(CollectDeviceChartDataSource!);
    }

    private Task<ChartDataSource> OnInitBusinessDevicePie()
    {
        var data = typeof(DeviceStatusEnum).ToSelectList();
        if (BusinessDeviceChartDataSource == null)
        {
            BusinessDeviceChartDataSource = new ChartDataSource();
            BusinessDeviceChartDataSource.Options.Title = Localizer["BusinessDevice"];
            BusinessDeviceChartDataSource.Options.ShowLegend = false;
            BusinessDeviceChartDataSource.Labels = data.Select(a => a.Text);
            BusinessDeviceChartDataSource.Data.Add(new ChartDataset()
            {
                Data = data.Select(i => GlobalData.ReadOnlyBusinessDevices.Count(device => device.Value.DeviceStatus == (DeviceStatusEnum)Enum.Parse(typeof(DeviceStatusEnum), i.Value))).Cast<object>()
            });
        }
        else
        {
            BusinessDeviceChartDataSource.Data[0].Data = data.Select(i => GlobalData.ReadOnlyBusinessDevices.Count(device => device.Value.DeviceStatus == (DeviceStatusEnum)Enum.Parse(typeof(DeviceStatusEnum), i.Value))).Cast<object>();
        }
        return Task.FromResult(BusinessDeviceChartDataSource!);
    }

    private Task<ChartDataSource> OnInitVariablePie()
    {
        var data = new List<bool>() { true, false };
        if (VariableChartDataSource == null)
        {
            VariableChartDataSource = new ChartDataSource();
            VariableChartDataSource.Options.Title = Localizer["Variable"];
            VariableChartDataSource.Options.ShowLegend = false;
            VariableChartDataSource.Labels = data.Select(a => a ? Localizer["OnLine"].Value : Localizer["OffLine"].Value);
            VariableChartDataSource.Data.Add(new ChartDataset()
            {
                Data = data.Select(i => GlobalData.ReadOnlyVariables.Count(device => device.Value.IsOnline == i)).Cast<object>()
            });
        }
        else
        {
            VariableChartDataSource.Data[0].Data = data.Select(i => GlobalData.ReadOnlyVariables.Count(device => device.Value.IsOnline == i)).Cast<object>();
        }
        return Task.FromResult(VariableChartDataSource!);
    }

    private Task<ChartDataSource> OnInitAlarmPie()
    {
        var data = new List<bool>() { true };
        if (AlarmChartDataSource == null)
        {
            AlarmChartDataSource = new ChartDataSource();
            AlarmChartDataSource.Options.Title = Localizer["Alarm"];
            AlarmChartDataSource.Options.ShowLegend = false;
            AlarmChartDataSource.Labels = data.Select(a => Localizer["AlarmCount"].Value);
            AlarmChartDataSource.Data.Add(new ChartDataset()
            {
                Data = new List<object>() { GlobalData.ReadOnlyRealAlarmVariables.Count() }
            });
        }
        else
        {
            AlarmChartDataSource.Data[0].Data = new List<object>() { GlobalData.ReadOnlyRealAlarmVariables.Count() };
        }
        return Task.FromResult(AlarmChartDataSource!);
    }

    #endregion 曲线
}