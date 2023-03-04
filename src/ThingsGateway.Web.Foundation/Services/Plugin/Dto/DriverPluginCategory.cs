namespace ThingsGateway.Web.Foundation
{
    public class DriverPluginCategory
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public List<DriverPluginCategory> Children { get; set; }
    }

}