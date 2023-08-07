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

// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://www.blazor.zone or https://argozhang.github.io/

using Microsoft.AspNetCore.Components;

namespace ThingsGateway.Admin.Blazor.Core;

/// <summary>
/// Ajax组件
/// </summary>
public class Ajax : ComponentBase, IDisposable
{
    private IJSObjectReference JSObjectReference;

    /// <summary>
    /// 获得/设置 IJSRuntime 实例
    /// </summary>
    [Inject]
    protected IJSRuntime JSRuntime { get; set; }

    [Inject]
    private AjaxService AjaxService { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void Dispose()
    {
        AjaxService.UnRegister(this);
        AjaxService.UnRegisterGoto(this);
        AjaxService.UnRegisterDownFile(this);
    }

    /// <summary>
    /// 请求并返回消息
    /// </summary>
    /// <param name="option">Ajax配置</param>
    /// <returns></returns>
    public async Task<string> GetMessageAsync(AjaxOption option)
    {
        var obj = await JSObjectReference.InvokeAsync<string>("blazor_ajax", option.Url, option.Method, option.Data);
        return obj;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            JSObjectReference = await JSRuntime.LoadModuleAsync("blazor_ajax");
        }
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        AjaxService.Register(this, GetMessageAsync);
        AjaxService.RegisterGoto(this, GotoAsync);
        AjaxService.RegisterDownFile(this, DownFileAsync);
    }

    private async Task GotoAsync(string url)
    {
        await JSObjectReference.InvokeVoidAsync("blazor_ajax_goto", url);
    }
    private async Task DownFileAsync(string url, string fileName, object dtoObject)
    {
        await JSObjectReference.InvokeVoidAsync("blazor_downloadFile", url, fileName, dtoObject);
    }

}