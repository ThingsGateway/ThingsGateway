namespace ThingsGateway.Web.Rcl.Core
{
    public class MobileComponentBase : CultureComponentBase
    {
        [CascadingParameter(Name = "IsMobile")]
        public bool IsMobile { get; set; }
    }
}