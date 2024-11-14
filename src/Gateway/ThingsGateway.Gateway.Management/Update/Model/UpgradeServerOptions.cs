using ThingsGateway.ConfigurableOptions;

namespace ThingsGateway.AutoUpdate;

public class UpgradeServerOptions : IConfigurableOptions
{
    public string UpgradeServerIP { get; set; }

    public int UpgradeServerPort { get; set; }
}
