namespace ThingsGateway.Foundation.Adapter.OPCUA;
public class OPCNode
{
    public string OPCUrl { get; set; }
    public int UpdateRate { get; set; } = 1000;
    public int GroupSize { get; set; } = 500;
    public float DeadBand { get; set; } = 0;
    public int ReconnectPeriod { get; set; } = 5000;
    
    public override string ToString()
    {
        return OPCUrl;
    }
}
