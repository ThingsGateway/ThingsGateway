using Microsoft.AspNetCore.Components;

namespace ThingsGateway.Web.Foundation;

public abstract class DriverUI : ComponentBase
{
    [Parameter]
    public virtual object Driver { get; set; }
    public abstract List<CollectDeviceVariable> GetVariableList();

}
