
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------




using BootstrapBlazor.Components;

using System.ComponentModel;
using System.Reflection;

using ThingsGateway.Core.Extension;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 变量分页查询参数
/// </summary>
public class VariablePageInput : BasePageInput
{
    /// <inheritdoc/>
    public string Name { get; set; }

    /// <inheritdoc/>
    public long? DeviceId { get; set; }

    /// <inheritdoc/>
    public string RegisterAddress { get; set; }

    /// <inheritdoc/>
    public long? BusinessDeviceId { get; set; }
}



public class VariableSearchInput : ITableSearchModel
{
    /// <inheritdoc/>
    public string Name { get; set; }

    /// <inheritdoc/>
    public long? DeviceId { get; set; }

    /// <inheritdoc/>
    public string RegisterAddress { get; set; }

    /// <inheritdoc/>
    public IEnumerable<IFilterAction> GetSearches()
    {
        var ret = new List<IFilterAction>();
        ret.AddIF(!string.IsNullOrEmpty(Name), () => new SearchFilterAction(nameof(Variable.Name), Name));
        ret.AddIF(!string.IsNullOrEmpty(RegisterAddress), () => new SearchFilterAction(nameof(Variable.RegisterAddress), RegisterAddress));
        ret.AddIF(DeviceId > 0, () => new SearchFilterAction(nameof(Variable.DeviceId), DeviceId, FilterAction.Equal));
        return ret;
    }

    /// <inheritdoc/>
    public void Reset()
    {
        Name = null;
        RegisterAddress = null;
        DeviceId = null;
    }
}

