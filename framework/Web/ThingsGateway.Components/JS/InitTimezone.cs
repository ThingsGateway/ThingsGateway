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

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ThingsGateway.Components;

/// <summary>
/// 获取Web客户端时差
/// </summary>
public class InitTimezone : IDisposable
{
    static readonly string _timezoneOffsetKey = "timezoneOffset";
    readonly IJSRuntime _jsRuntime;
    readonly CookieStorage _storage;
    TimeSpan _timezoneOffset;
    IJSObjectReference _helper;

    /// <summary>
    /// 当前的客户端时差
    /// </summary>
    public TimeSpan TimezoneOffset
    {
        get => _timezoneOffset;
        set
        {
            _storage.SetItemAsync(_timezoneOffsetKey, value.TotalMinutes);
            _timezoneOffset = value;
        }
    }
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="jsRuntime"></param>
    /// <param name="storage"></param>
    /// <param name="serviceProvider"></param>
    public InitTimezone(IJSRuntime jsRuntime, CookieStorage storage, IServiceProvider serviceProvider)
    {
        IHttpContextAccessor httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
        _jsRuntime = jsRuntime;
        _storage = storage;
        var httpContext = httpContextAccessor?.HttpContext;
        if (httpContext is not null)
        {
            var timezoneOffsetResult = httpContext.Request.Cookies[_timezoneOffsetKey];
            _timezoneOffset = TimeSpan.FromMinutes(Convert.ToDouble(timezoneOffsetResult));
        }
    }
    /// <summary>
    /// 获取Web客户端时差
    /// </summary>
    /// <returns></returns>
    public async Task SetTimezoneOffsetAsync()
    {
        var timezoneOffsetResult = await _storage.GetCookieAsync(_timezoneOffsetKey);
        if (string.IsNullOrEmpty(timezoneOffsetResult) is false)
        {
            TimezoneOffset = TimeSpan.FromMinutes(Convert.ToDouble(timezoneOffsetResult));
            return;
        }
        _helper ??= await _jsRuntime.InvokeAsync<IJSObjectReference>("import", $"{BlazorResourceConst.ResourceUrl}js/getTimezoneOffset.js");
        var offset = await _helper.InvokeAsync<double>("getTimezoneOffset");
        TimezoneOffset = TimeSpan.FromMinutes(-offset);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _helper?.DisposeAsync();
    }
}
