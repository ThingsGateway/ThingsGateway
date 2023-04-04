using Newtonsoft.Json;

namespace IoTSharp.Data
{
    public class RpcResponse
    {
        public string DeviceId { get; set; }
        public string Method { get; set; }
        public string ResponseId { get; set; }
        public string Data { get; set; }
        public bool Success { get; set; }
    }

}
