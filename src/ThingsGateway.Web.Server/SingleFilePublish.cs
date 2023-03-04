namespace ThingsGateway.Web.Entry
{
    public class SingleFilePublish : ISingleFilePublish
    {
        public Assembly[] IncludeAssemblies()
        {
            return Array.Empty<Assembly>();
        }

        public string[] IncludeAssemblyNames()
        {
            return new[]
            {
            "ThingsGateway.Foundation",
            "ThingsGateway.Web.Foundation",
            "ThingsGateway.Web.Page",

            "ThingsGateway.Web.Rcl",
            "ThingsGateway.Web.Rcl.Core",
            "ThingsGateway.Core",
            "ThingsGateway.Web.Core"
        };
        }
    }
}