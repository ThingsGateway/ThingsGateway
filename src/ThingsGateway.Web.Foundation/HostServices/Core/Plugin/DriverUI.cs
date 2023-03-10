using Microsoft.AspNetCore.Components;

namespace ThingsGateway.Web.Foundation;

public class DriverUI : ComponentBase
{
    [Parameter]
    public virtual object Driver { get; set; }

}
