namespace ThingsGateway.Web.Rcl.Core
{
    public class Filters
    {
        public string Key { get; set; }
        public string Title { get; set; }
        public bool Value { get; set; }
    }

    public class PageSize
    {
        public string Key { get; set; }
        public int Value { get; set; }
    }

    public class StringFilters
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}