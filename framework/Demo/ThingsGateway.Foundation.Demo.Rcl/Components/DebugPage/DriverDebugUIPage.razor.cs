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

using BlazorComponent;

using Microsoft.AspNetCore.Components;

using System.Collections.Generic;

namespace ThingsGateway.Foundation.Demo;


/// <inheritdoc/>
public partial class DriverDebugUIPage : DriverDebugUIBase
{
    /// <summary>
    /// DeviceVariableRunTimes
    /// </summary>
    public List<DeviceVariableRunTime> DeviceVariableRunTimes;
    /// <summary>
    /// MaxPack
    /// </summary>
    public int MaxPack = 100;
    private StringNumber _selected = 0;

    /// <inheritdoc/>
    ~DriverDebugUIPage()
    {
        this.SafeDispose();
    }

    /// <summary>
    /// 自定义模板
    /// </summary>
    [Parameter]
    public RenderFragment CodeContent { get; set; }

    /// <summary>
    /// 自定义模板
    /// </summary>
    [Parameter]
    public RenderFragment OtherContent { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override IReadWrite Plc { get; set; }

    /// <summary>
    /// 自定义模板
    /// </summary>
    [Parameter]
    public RenderFragment ReadWriteContent { get; set; }

    /// <summary>
    /// Sections
    /// </summary>
    [Parameter]
    public List<(string Code, string Language)> Sections { get; set; } = new();

    /// <summary>
    /// ShowDefaultOtherContent
    /// </summary>
    [Parameter]
    public bool ShowDefaultOtherContent { get; set; } = true;

    /// <inheritdoc/>
    public override void Dispose()
    {
        Plc?.SafeDispose();
        base.Dispose();
    }

    /// <summary>
    /// MulReadAsync
    /// </summary>
    /// <returns></returns>
    public async Task MulReadAsync()
    {
        var deviceVariableSourceReads = Plc.LoadSourceRead<DeviceVariableSourceRead, DeviceVariableRunTime>(DeviceVariableRunTimes, MaxPack, 1000);
        foreach (var item in deviceVariableSourceReads)
        {
            var result = await Plc.ReadAsync(item.Address, item.Length);
            if (result.IsSuccess)
            {
                try
                {
                    item.DeviceVariableRunTimes.PraseStructContent(Plc, result.Content);
                    Messages.Add((Microsoft.Extensions.Logging.LogLevel.Information, DateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat(InitTimezone.TimezoneOffset) + " - " + result.Content.ToHexString(' ')));
                }
                catch (Exception ex)
                {
                    Messages.Add((Microsoft.Extensions.Logging.LogLevel.Warning, DateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat(InitTimezone.TimezoneOffset) + " - " + ex));
                }

            }
            else
                Messages.Add((Microsoft.Extensions.Logging.LogLevel.Warning, DateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat(InitTimezone.TimezoneOffset) + " - " + result.Message));
        }
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="firstRender"></param>
    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);
    }

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        DeviceVariableRunTimes = new()
            {
                                new DeviceVariableRunTime()
                                {
                                    DataTypeEnum=DataTypeEnum.Int16,
                                    Address="40001",
                                    IntervalTime=1000,
                                },
                                   new DeviceVariableRunTime()
                                {
                                    DataTypeEnum=DataTypeEnum.Int16,
                                    Address="40011",
                                    IntervalTime=1000,
                                },
                                   new DeviceVariableRunTime()
                                {
                                    DataTypeEnum=DataTypeEnum.Int16,
                                    Address="40031",
                                    IntervalTime=1000,
                                },
                                   new DeviceVariableRunTime()
                                {
                                    DataTypeEnum=DataTypeEnum.Int16,
                                    Address="40101",
                                    IntervalTime=1000,
                                },
            };
        Sections.Add((
"""
                /// <inheritdoc/>
                public class DeviceVariableSourceRead : IDeviceVariableSourceRead<DeviceVariableRunTime>
                {
                    /// <inheritdoc/>
                    public TimerTick TimerTick { get; set; }
                    /// <inheritdoc/>
                    public string Address { get; set; }
                    /// <inheritdoc/>
                    public int Length { get; set; }
                    /// <inheritdoc/>
                    public List<DeviceVariableRunTime> DeviceVariableRunTimes { get; set; } = new List<DeviceVariableRunTime>();
                }
                /// <inheritdoc/>
                public class DeviceVariableRunTime : IDeviceVariableRunTime
                {
                    /// <inheritdoc/>
                    [Description("读取间隔")]
                    public int IntervalTime { get; set; }
                    /// <inheritdoc/>
                    [Description("变量地址")]
                    public string Address { get; set; }
                    /// <inheritdoc/>
                    public int Index { get; set; }
                    /// <inheritdoc/>
                    public IThingsGatewayBitConverter ThingsGatewayBitConverter { get; set; }
                    /// <inheritdoc/>
                    [Description("数据类型")]
                    public DataTypeEnum DataTypeEnum { get; set; }
                    /// <inheritdoc/>
                    [Description("实时值")]
                    public object Value { get; set; }
                    /// <inheritdoc/>
                    public OperResult SetValue(object value)
                    {
                        Value = value;
                        return OperResult.CreateSuccessResult();
                    }
                }
                public List<DeviceVariableRunTime> DeviceVariableRunTimes;
                                
                private static async Task ModbusClientAsync(IReadWrite plc)
                {
                DeviceVariableRunTimes = new()
                {
                                new DeviceVariableRunTime()
                                {
                                    DataTypeEnum=DataTypeEnum.Int16,
                                    Address="40001",
                                    IntervalTime=1000,
                                },
                                   new DeviceVariableRunTime()
                                {
                                    DataTypeEnum=DataTypeEnum.Int16,
                                    Address="40011",
                                    IntervalTime=1000,
                                },
                                   new DeviceVariableRunTime()
                                {
                                    DataTypeEnum=DataTypeEnum.Int16,
                                    Address="40031",
                                    IntervalTime=1000,
                                },
                                   new DeviceVariableRunTime()
                                {
                                    DataTypeEnum=DataTypeEnum.Int16,
                                    Address="40101",
                                    IntervalTime=1000,
                                },
                };

                    #region 连读
                var deviceVariableSourceReads = Plc.LoadSourceRead<DeviceVariableSourceRead, DeviceVariableRunTime>(DeviceVariableRunTimes, MaxPack);
                foreach (var item in deviceVariableSourceReads)
                {
                    var result = await Plc.ReadAsync(item.Address, item.Length);
                    if (result.IsSuccess)
                    {
                        item.DeviceVariableRunTimes.PraseStructContent(result.Content);
                    }
                }
                    #endregion

                }
                
""", "csharp"));
        base.OnInitialized();
    }
}