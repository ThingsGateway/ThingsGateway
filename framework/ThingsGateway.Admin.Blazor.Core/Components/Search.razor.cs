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

using ThingsGateway.Admin.Core;

namespace ThingsGateway.Admin.Blazor.Core;
/// <summary>
/// Search
/// </summary>
public partial class Search
{
    private string _value;
    private string Value
    {
        get => _value;
        set
        {
            _value = value;
            if (!string.IsNullOrEmpty(value))
            {
                NavigationManager.NavigateTo(value);
            }
        }
    }

    [Inject]
    private NavigationManager NavigationManager { get; set; }

    private List<SysResource> AvalidMenus;
    [Inject]
    private UserResoures UserResoures { get; set; }

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        AvalidMenus = UserResoures.SameLevelMenus.Where(it => it.Component != null).ToList();
        base.OnParametersSet();
    }
}