namespace ThingsGateway.Web.Entry
{
    /// <inheritdoc cref="ISingleFilePublish"/>
    public class SingleFilePublish : ISingleFilePublish
    {
        /// <inheritdoc/>
        public Assembly[] IncludeAssemblies()
        {
            return Array.Empty<Assembly>();
        }

        /// <inheritdoc/>
        public string[] IncludeAssemblyNames()
        {
            return new[]
            {
            "ThingsGateway.Foundation",
            "ThingsGateway.Web.Foundation",
            "ThingsGateway.Web.Page",
            "ThingsGateway.Application",
            "ThingsGateway.Core",
            "ThingsGateway.Web.Rcl",
            "ThingsGateway.Web.Rcl.Core",
            "ThingsGateway.Web.Core"
        };
        }
    }
}