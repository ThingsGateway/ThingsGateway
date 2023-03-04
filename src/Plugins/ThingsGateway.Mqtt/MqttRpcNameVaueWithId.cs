namespace ThingsGateway.Mqtt
{
    public class MqttRpcNameVaueWithId
    {
        public string RpcId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
    public class MqttRpcResult
    {
        public string RpcId { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
    }
}
