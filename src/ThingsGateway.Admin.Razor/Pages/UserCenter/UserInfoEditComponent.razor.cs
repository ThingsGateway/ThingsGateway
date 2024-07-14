//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Components.Forms;

using ThingsGateway.Admin.Application;

namespace ThingsGateway.Admin.Razor;

public partial class UserInfoEditComponent
{
    [Parameter]
    [EditorRequired]
    public SysUser Model { get; set; }

    [Parameter]
    [EditorRequired]
    public Func<EditContext, Task> OnSave { [return: NotNull] get; set; }

    #region 头像

    private List<UploadFile> PreviewFileList;

    [FileValidation(Extensions = [".png", ".jpg", ".jpeg"], FileSize = 200 * 1024)]
    public IBrowserFile? Picture { get; set; }

    private CancellationTokenSource? ReadAvatarToken { get; set; }

    public void Dispose()
    {
        ReadAvatarToken?.Cancel();
        GC.SuppressFinalize(this);
    }

    protected override Task OnParametersSetAsync()
    {
        PreviewFileList = new(new[] { new UploadFile { PrevUrl = Model.Avatar } });
        return base.OnParametersSetAsync();
    }

    private async Task OnAvatarUpload(UploadFile file)
    {
        if (file != null && file.File != null)
        {
            var format = file.File.ContentType;
            ReadAvatarToken ??= new CancellationTokenSource();
            if (ReadAvatarToken.IsCancellationRequested)
            {
                ReadAvatarToken.Dispose();
                ReadAvatarToken = new CancellationTokenSource();
            }

            await file.RequestBase64ImageFileAsync(format, 640, 480, 1024 * 200, ReadAvatarToken.Token);

            if (file.Code != 0)
            {
                await ToastService.Error($"{file.Error} ");
            }
            else
            {
                Model.Avatar = file.PrevUrl;
            }
        }
    }

    #endregion 头像
}
