//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Gateway.Management;

using TouchSocket.Core;

namespace ThingsGateway.AutoUpdate;

/// <inheritdoc/>
public partial class UpdateZipFileInfo
{

    [Parameter]
    public string ClassString { get; set; }

    [Parameter]
    public string HeaderText { get; set; }

    [Parameter, EditorRequired]
    public string LogPath { get; set; }
    [Parameter, EditorRequired]
    public UpdateZipFile Model { get; set; }
    [Parameter, EditorRequired]
    public ILog Logger { get; set; }

    [Inject]
    private IUpdateZipFileService UpdateZipFileService { get; set; }
    [Inject]
    [NotNull]
    public IStringLocalizer<ThingsGateway.Gateway.Management._Imports> ManagementLocalizer { get; set; }
    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        HeaderText = ManagementLocalizer[nameof(HeaderText)];
    }
    [Inject]
    private SwalService SwalService { get; set; }
    private async Task OnRestart()
    {
        var result = await SwalService.ShowModal(new SwalOption()
        {
            Category = SwalCategory.Warning,
            Title = ManagementLocalizer["Restart"]
        });
        if (result)
        {
            RestartServerHelper.RestartServer();
        }
    }
    private async Task OnUpdate()
    {
        await UpdateZipFileService.Update(Model, async () =>
          {
              var result = await SwalService.ShowModal(new SwalOption()
              {
                  Category = SwalCategory.Warning,
                  Title = ManagementLocalizer["Restart"]
              });
              return result;
          });
    }

}
