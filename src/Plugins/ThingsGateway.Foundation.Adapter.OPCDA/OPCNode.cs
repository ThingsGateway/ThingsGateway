namespace ThingsGateway.Foundation.Adapter.OPCDA;
public class OPCNode
{
    public string OPCIP { get; set; }
    public string OPCName { get; set; }
    public bool ActiveSubscribe { get; set; } = false;
    public int UpdateRate { get; set; } = 1000;
    public int GroupSize { get; set; } = 500;
    public float DeadBand { get; set; } = 0;
    public int CheckRate { get; set; } = 600000;

    public override string ToString()
    {
        return $"{(OPCIP.IsNullOrEmpty() ? "localhost" : OPCIP)}:{OPCName}";
    }
}
