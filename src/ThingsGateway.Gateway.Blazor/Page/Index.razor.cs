//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Masa.Blazor;

using Microsoft.Extensions.DependencyInjection;

using SqlSugar;

using ThingsGateway.Admin.Application;
using ThingsGateway.Core.Extension;
using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Gateway.Blazor;

/// <summary>
/// 首页
/// </summary>
#if Admin

public partial class Index
#else

[Route("/index")]
public partial class Index
#endif
{
    private List<SysOperateLog> _sysOperateLogs;

    private List<SysVisitLog> _sysVisitLogs;

    private List<RpcLog> _rpcLogs;

    private List<BackendLog> _backendLogs;
    private int _collectDeviceCount;
    private int _businessDeviceCount;
    private int _variableCount;
    private int _alarmCount;
    private object _collectDeviceChart = new();
    private object _businessDeviceChart = new();
    private object _variableChart = new();
    private HardwareInfoWorker _hardwareInfoWorker { get; set; }
    private readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromSeconds(5));

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        _periodicTimer?.Dispose();
        base.Dispose(disposing);
    }

    [Inject]
    private GlobalData GlobalData { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void OnInitialized()
    {
        _hardwareInfoWorker = WorkerUtil.GetWoker<HardwareInfoWorker>();
        _ = RunTimerAsync();
        base.OnInitialized();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await SetOption();
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task RunTimerAsync()
    {
        while (await _periodicTimer.WaitForNextTickAsync())
        {
            await SetOption();
        }
    }

    private async Task SetOption()
    {
        try
        {
            await GetHardwareInfoEChartsOption();
            _sysVisitLogs = (await _serviceScope.ServiceProvider.GetService<IVisitLogService>().PageAsync(new() { Size = 5, Account = UserResoures.CurrentUser?.Account })).Records.ToList();
            _sysOperateLogs = (await _serviceScope.ServiceProvider.GetService<IOperateLogService>().PageAsync(new() { Size = 5, Account = UserResoures.CurrentUser?.Account })).Records.ToList();
            _rpcLogs = (await _serviceScope.ServiceProvider.GetService<IRpcLogService>().PageAsync(new() { Size = 5, })).Records.ToList();
            _backendLogs = (await _serviceScope.ServiceProvider.GetService<IBackendLogService>().PageAsync(new() { Size = 5, })).Records.ToList();

            _collectDeviceCount = GlobalData.CollectDevices.Count;
            _businessDeviceCount = GlobalData.BusinessDevices.Count;
            _variableCount = GlobalData.AllVariables.Count();
            _alarmCount = WorkerUtil.GetWoker<AlarmWorker>().RealAlarmVariables.Count;
            showCollectDeviceChart();
            showBusinessDeviceChart();
            showVariableChart();
            await InvokeAsync(StateHasChanged);
            await Task.Delay(5000);
        }
        catch
        {
        }
    }


    #region CPU曲线
    private object hardwareInfoEChartsOption;
    private MECharts hardwareInfoECharts;
    private async Task GetHardwareInfoEChartsOption()
    {
        var hisDatas = _hardwareInfoWorker.GetHis();
        if (hardwareInfoEChartsOption == null)
        {
            hardwareInfoEChartsOption ??= new
            {
                backgroundColor = "",
                tooltip = new { trigger = "axis" },
                legend = new
                {
                    data = new[] {
                AppService.I18n.T("CPU使用率"),
                AppService.I18n.T("内存使用率"),
                AppService.I18n.T("磁盘使用率"),
                AppService.I18n.T("温度"),
                AppService.I18n.T("电池"),
                AppService.I18n.T("上载速度"),
                AppService.I18n.T("下载速度")
            },
                    left = "5%"
                },
                grid = new { left = "3%", right = "4%", bottom = "3%", containLabel = true },
                toolbox = new { feature = new { saveAsImage = new { } } },
                xAxis = new
                {
                    type = "category",
                    boundaryGap = false,
                    data = hisDatas.Select(a => a.Date.ToString("yyyy-MM-dd HH:mm")).ToArray()
                },
                yAxis = new { type = "value" },
                series = new[]
{
        new { name =AppService.I18n.T( "CPU使用率"), type = "line",
                data = hisDatas.Select(a=>a.CpuUsage).ToArray(),
            smooth=true
        },
        new { name = AppService.I18n.T("内存使用率"), type = "line",
                data = hisDatas.Select(a=>a.MemoryUsage).ToArray(),
            smooth=true
        },
        new { name = AppService.I18n.T("磁盘使用率"), type = "line",
                data = hisDatas.Select(a=>a.DriveUsage).ToArray(),
            smooth=true
        },
        new { name = AppService.I18n.T("温度"), type = "line",
                data = hisDatas.Select(a=>a.Temperature).ToArray(),
            smooth=true
        },
                new { name = AppService.I18n.T("电池"), type = "line",
                data = hisDatas.Select(a=>a.Battery).ToArray(),
            smooth=true
        },
        new { name = AppService.I18n.T("上载速度"), type = "line",
                data = hisDatas.Select(a=>a.UplinkSpeed).ToArray(),
            smooth=true
        },
        new { name = AppService.I18n.T("下载速度"), type = "line",
                data = hisDatas.Select(a=>a.DownlinkSpeed).ToArray(),
            smooth=true
        },
    }
            };

        }
        else
        {
            var option = new
            {
                xAxis = new
                {
                    data = hisDatas.Select(a => a.Date.ToString("yyyy-MM-dd HH:mm")).ToArray()
                },
                series = new[]
  {
        new { name =AppService.I18n.T( "CPU使用率"),
                data = hisDatas.Select(a=>a.CpuUsage).ToArray(),
        },
        new { name = AppService.I18n.T("内存使用率"),
                data = hisDatas.Select(a=>a.MemoryUsage).ToArray(),
        },
        new { name = AppService.I18n.T("磁盘使用率"),
                data = hisDatas.Select(a=>a.DriveUsage).ToArray(),
        },
        new { name = AppService.I18n.T("温度"),
                data = hisDatas.Select(a=>a.Temperature).ToArray(),
        },
                new { name = AppService.I18n.T("电池"),
                data = hisDatas.Select(a=>a.Battery).ToArray(),
        },
        new { name = AppService.I18n.T("上载速度"),
                data = hisDatas.Select(a=>a.UplinkSpeed).ToArray(),
        },
        new { name = AppService.I18n.T("下载速度"),
                data = hisDatas.Select(a=>a.DownlinkSpeed).ToArray(),
        },
    }
            };
            await hardwareInfoECharts.SetOption(option, false, false);

        }
    }
    #endregion

    private void showCollectDeviceChart()
    {
        var statusCounts = typeof(DeviceStatusEnum).GetEnumList()
    .Select(status => new
    {
        Name = status.Description,
        Count = GlobalData.CollectDevices.Count(device => device.DeviceStatus == (DeviceStatusEnum)status.Value)
    })
    .ToList();
        List<object> datas = new();
        foreach (var item in statusCounts)
        {
            var data = new { value = item.Count, name = item.Name };
            datas.Add(data);
        }

        _collectDeviceChart = new
        {
            backgroundColor = "",
            tooltip = new
            {
                trigger = "item"
            },

            series = new[]
     {
        new
        {
            name = AppService.I18n.T("设备状态"),
            type = "pie",
            radius= "90%",
            label = new
            {
                show = false,
                formatter="{b}: {c}"
            },
            data = datas,
            emphasis = new
            {
                itemStyle = new
                {
                    shadowBlur = 10,
                    shadowOffsetX = 0,
                    shadowColor = "rgba(0, 0, 0, 0.5)"
                }
            }
        }
    }
        };
    }

    private void showBusinessDeviceChart()
    {
        var statusCounts = typeof(DeviceStatusEnum).GetEnumList()
    .Select(status => new
    {
        Name = status.Description,
        Count = GlobalData.BusinessDevices.Count(device => device.DeviceStatus == (DeviceStatusEnum)status.Value)
    })
    .ToList();
        List<object> datas = new();
        foreach (var item in statusCounts)
        {
            var data = new { value = item.Count, name = item.Name };
            datas.Add(data);
        }

        _businessDeviceChart = new
        {
            backgroundColor = "",
            tooltip = new
            {
                trigger = "item"
            },
            series = new[]
     {
        new
        {
            name = AppService.I18n.T("设备状态"),
            type = "pie",
            radius= "90%",
            label = new
            {
                show = false,
                formatter="{b}: {c}"
            },
            data = datas,
            emphasis = new
            {
                itemStyle = new
                {
                    shadowBlur = 10,
                    shadowOffsetX = 0,
                    shadowColor = "rgba(0, 0, 0, 0.5)"
                }
            }
        }
    }
        };
    }

    private void showVariableChart()
    {
        var statusCounts = new List<bool>() { false, true }
    .Select(status => new
    {
        Name = status ? "Online" : "Offline",
        Count = GlobalData.AllVariables.Count(device => device.IsOnline == status)
    })
    .ToList();
        List<object> datas = new();
        foreach (var item in statusCounts)
        {
            var data = new { value = item.Count, name = item.Name };
            datas.Add(data);
        }

        _variableChart = new
        {
            backgroundColor = "",
            tooltip = new
            {
                trigger = "item"
            },
            series = new[]
     {
        new
        {
            name = AppService.I18n.T("变量状态"),
            type = "pie",
            radius= "90%",
            label = new
            {
                show = false,
                formatter="{b}: {c}"
            },
            data = datas,
            emphasis = new
            {
                itemStyle = new
                {
                    shadowBlur = 10,
                    shadowOffsetX = 0,
                    shadowColor = "rgba(0, 0, 0, 0.5)"
                }
            }
        }
    }
        };
    }
}