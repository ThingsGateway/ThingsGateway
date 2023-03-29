using Microsoft.AspNetCore.Components;

namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 导入变量UI
/// </summary>
public abstract class DriverUI : ComponentBase
{
    /// <summary>
    /// 设备通讯类
    /// </summary>
    [Parameter]
    public virtual object Driver { get; set; }
    /// <summary>
    /// 获取导入变量列表
    /// </summary>
    /// <returns></returns>
    public abstract List<CollectDeviceVariable> GetVariableList();

}
