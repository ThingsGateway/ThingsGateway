﻿#region copyright
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

using Masa.Blazor;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace ThingsGateway.Components;
/// <summary>
/// Razor组件
/// </summary>
public abstract class BaseComponentBase : ComponentBase, IDisposable
{

    /// <summary>
    /// 级联更新
    /// </summary>
    [CascadingParameter(Name = "Changed")]
    public bool Changed { get; set; }

    /// <summary>
    /// 是否手机端
    /// </summary>
    [CascadingParameter(Name = "IsMobile")]
    public bool IsMobile { get; set; }

    /// <summary>
    /// MasaBlazor
    /// </summary>
    [Inject]
    public MasaBlazor MasaBlazor { get; set; }

    /// <summary>
    /// 弹出层服务
    /// </summary>
    [Inject]
    public IPopupService PopupService { get; set; }

    protected IServiceScope _serviceScope { get; set; }

    [Inject]
    private IServiceScopeFactory _serviceScopeFactory { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public virtual void Dispose()
    {
    }

    /// <summary>
    /// InvokeAsync(StateHasChanged)
    /// </summary>
    /// <returns></returns>
    protected virtual Task InvokeStateHasChangedAsync()
    {
        return InvokeAsync(StateHasChanged);
    }

    protected override void OnInitialized()
    {
        _serviceScope = _serviceScopeFactory.CreateScope();
    }
}