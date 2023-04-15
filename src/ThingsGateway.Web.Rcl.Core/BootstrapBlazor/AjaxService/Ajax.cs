// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://www.blazor.zone or https://argozhang.github.io/

namespace ThingsGateway.Web.Rcl.Core
{
    public class Ajax : ComponentBase, IDisposable
    {
        private IJSObjectReference JSObjectReference;

        /// <summary>
        /// ���/���� IJSRuntime ʵ��
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
        /// Dispose ����
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
        /// OnInitialized ����
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