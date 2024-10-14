//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

using ThingsGateway.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.Debug;

/// <inheritdoc/>
public partial class AdapterDebugComponent : AdapterDebugBase
{
    /// <summary>
    /// MaxPack
    /// </summary>
    public int MaxPack = 100;

    /// <summary>
    /// VariableRunTimes
    /// </summary>
    public List<VariableClass> VariableRunTimes;

    [Parameter]
    public string ClassString { get; set; }

    [Parameter]
    public string HeaderText { get; set; }

    [Parameter, EditorRequired]
    public string LogPath { get; set; }

    [Parameter]
    public ILog Logger { get; set; }

    /// <summary>
    /// 自定义模板
    /// </summary>
    [Parameter]
    public RenderFragment OtherContent { get; set; }

    /// <summary>
    /// 自定义模板
    /// </summary>
    [Parameter]
    public RenderFragment ReadWriteContent { get; set; }

    [Parameter]
    public bool ShowDefaultOtherContent { get; set; } = true;

    [Parameter]
    public bool ShowDefaultReadWriteContent { get; set; } = true;

    [Inject]
    private IStringLocalizer<AdapterDebugComponent> Localizer { get; set; }

    /// <summary>
    /// MulReadAsync
    /// </summary>
    /// <returns></returns>
    public async Task MulRead()
    {
        if (Plc != null)
        {
            var deviceVariableSourceReads = Plc.LoadSourceRead<VariableSourceClass>(VariableRunTimes, MaxPack, 1000);
            foreach (var item in deviceVariableSourceReads)
            {
                var result = await Plc.ReadAsync(item.RegisterAddress, item.Length);
                if (result.IsSuccess)
                {
                    try
                    {
                        var result1 = item.VariableRunTimes.PraseStructContent(Plc, result.Content, exWhenAny: true);
                        if (!result1.IsSuccess)
                        {
                            item.LastErrorMessage = result1.ErrorMessage;
                            var time = DateTime.Now;
                            item.VariableRunTimes.ForEach(a => a.SetValue(null, time, isOnline: false));
                            Plc.Logger?.Warning(result1.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        Plc.Logger?.Exception(ex);
                    }
                }
                else
                {
                    item.LastErrorMessage = result.ErrorMessage;
                    var time = DateTime.Now;
                    item.VariableRunTimes.ForEach(a => a.SetValue(null, time, isOnline: false));
                    Plc.Logger?.Warning(result.ToString());
                }
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        VariableRunTimes = new()
            {
                                new VariableClass()
                                {
                                    DataType=DataTypeEnum.Int16,
                                    RegisterAddress="40001",
                                    IntervalTime=1000,
                                },
                                   new VariableClass()
                                {
                                    DataType=DataTypeEnum.Int32,
                                    RegisterAddress="40011",
                                    IntervalTime=1000,
                                },
            };

        HeaderText = Localizer[nameof(HeaderText)];
        base.OnInitialized();
    }
}
