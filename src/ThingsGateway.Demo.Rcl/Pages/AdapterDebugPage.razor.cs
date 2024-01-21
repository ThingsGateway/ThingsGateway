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

using ThingsGateway.Components;
using ThingsGateway.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.Demo;

/// <inheritdoc/>
public partial class AdapterDebugPage : AdapterDebugBase
{
    /// <summary>
    /// VariableRunTimes
    /// </summary>
    public List<IVariable> VariableRunTimes;

    /// <summary>
    /// MaxPack
    /// </summary>
    public int MaxPack = 100;

    /// <inheritdoc/>
    ~AdapterDebugPage()
    {
        this.SafeDispose();
    }

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

    /// <summary>
    /// MulReadAsync
    /// </summary>
    /// <returns></returns>
    public async Task MulReadAsync()
    {
        if (Plc != null)
        {
            var deviceVariableSourceReads = Plc.LoadSourceRead<VariableSourceDemo>(VariableRunTimes, MaxPack, 1000);
            deviceVariableSourceReads.ForEach(a => a.VariableRunTimes.ForEach(b => b.VariableSource = a));
            foreach (var item in deviceVariableSourceReads)
            {
                var result = await Plc.ReadAsync(item.RegisterAddress, item.Length);
                if (result.IsSuccess)
                {
                    try
                    {
                        item.VariableRunTimes.PraseStructContent(Plc, result.Content, item, exWhenAny: true);
                    }
                    catch (Exception ex)
                    {
                        Plc.Logger.Exception(ex);
                    }
                }
                else
                {
                    item.LastErrorMessage = result.ErrorMessage;
                    item.VariableRunTimes.ForEach(a => a.SetValue(null, isOnline: false));
                    Plc.Logger.Warning(result.ToString());
                }
            }
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
        VariableRunTimes = new()
            {
                                new VariableDemo()
                                {
                                    DataType=DataTypeEnum.Int16,
                                    RegisterAddress="40001",
                                    IntervalTime=1000,
                                },
                                   new VariableDemo()
                                {
                                    DataType=DataTypeEnum.Int32,
                                    RegisterAddress="40011",
                                    IntervalTime=1000,
                                },
            };
        base.OnInitialized();
    }

    [Inject]
    private IPlatformService PlatformService { get; set; }

    private async Task OnExportClick()
    {
        if (!LogPath.IsNullOrEmpty())
            await PlatformService.OnLogExport(LogPath);
    }

    private StringNumber tab;
    private List<(int, string)> Messages { get; set; }

    [Parameter, EditorRequired]
    public string LogPath { get; set; }

    /// <summary>
    /// Height
    /// </summary>
    [Parameter, EditorRequired]
    public StringNumber Height { get; set; } = 550;

    protected override async Task ExecuteAsync()
    {
        try
        {
            if (LogPath != null)
            {
                var files = TextFileReader.GetFile(LogPath);
                if (files == null || files.FirstOrDefault() == null || !files.FirstOrDefault().IsSuccess)
                {
                }
                else
                {
                    var result = TextFileReader.LastLog(files.FirstOrDefault().FullName, 0);
                    if (result.IsSuccess)
                    {
                        Messages = result.Content.Select(a => ((int)a.LogLevel, $"{a.LogTime} - {a.Message} {Environment.NewLine} {a.ExceptionString}")).ToList();
                    }
                    else
                    {
                    }
                }
            }
        }
        catch
        {
        }
        await base.ExecuteAsync();
    }
}