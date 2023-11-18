namespace ThingsGateway.Foundation.Rpc
{
    internal class ProxyModel
    {
        public MethodInstance MethodInstance { get; set; }
        public string InvokeKey { get; set; }
        public bool InvokeOption { get; set; }
        public Method GenericMethod { get; set; }
    }
}
