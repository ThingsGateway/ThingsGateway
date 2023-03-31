using Newtonsoft.Json.Linq;

namespace ThingsGateway.Foundation.Extension.Json
{
    /// <summary>
    /// JsonExtension
    /// </summary>
    public static class JsonExtensions
    {
        /// <summary>
        /// JSON∏Ò ΩªØ
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static string FormatJson(this string json)
        { return JToken.Parse(json).ToString(); }
    }
}