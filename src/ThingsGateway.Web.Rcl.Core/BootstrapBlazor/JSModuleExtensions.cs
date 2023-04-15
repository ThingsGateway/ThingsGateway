// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://www.blazor.zone or https://argozhang.github.io/

global using Microsoft.JSInterop;

namespace ThingsGateway.Web.Rcl.Core
{
    /// <summary>
    /// JSModule extensions class
    /// </summary>
    public static class JSModuleExtensions
    {
        /// <summary>
        /// IJSRuntime 扩展方法 动态加载脚本
        /// </summary>
        /// <param name="jsRuntime"></param>
        /// <param name="fileName"></param>
        /// <param name="relative">是否为相对路径 默认 true</param>
        /// <returns></returns>
        public static async Task<IJSObjectReference> LoadModuleAsync(this IJSRuntime jsRuntime, string fileName, bool relative = true)
        {
            var filePath = relative ? BlazorConst.ResourceUrl + $"js/{fileName}.js" : fileName;
            try
            {
                return await jsRuntime.InvokeAsync<IJSObjectReference>(identifier: "import", filePath);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}