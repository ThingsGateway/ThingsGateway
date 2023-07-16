#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/dotnetchina/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://www.blazor.zone or https://argozhang.github.io/

namespace ThingsGateway.Web.Rcl.Core
{
    public class Ajax : ComponentBase, IDisposable
    {
        private IJSObjectReference JSObjectReference;

        /// <summary>
        /// 获得/设置 IJSRuntime 实例
        /// </summary>
        [Inject]
        [NotNull]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        [NotNull]
        private AjaxService AjaxService { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task<string> GetMessageAsync(AjaxOption option)
        {
            var obj = await JSObjectReference.InvokeAsync<string>("tg_ajax", option.Url, option.Method, option.Data);
            return obj;
        }

        /// <summary>
        /// Dispose 方法
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                AjaxService.UnRegister(this);
                AjaxService.UnRegisterGoto(this);
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                JSObjectReference = await JSRuntime.LoadModuleAsync("Ajax");
            }
        }

        /// <summary>
        /// OnInitialized 方法
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            AjaxService.Register(this, GetMessageAsync);
            AjaxService.RegisterGoto(this, GotoAsync);
        }

        private async Task GotoAsync(string url)
        {
            await JSObjectReference.InvokeVoidAsync("tg_ajax_goto", url);
        }
    }
}