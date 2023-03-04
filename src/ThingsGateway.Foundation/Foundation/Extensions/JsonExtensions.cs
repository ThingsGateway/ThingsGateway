using Newtonsoft.Json.Linq;

namespace ThingsGateway.Foundation.Extension.Json
{
    /// <summary>
    /// JsonExtension
    /// </summary>
    public static class JsonExtensions
    {
        public static string FormatJson(this string json)
        { return JToken.Parse(json).ToString(); }
    }
}