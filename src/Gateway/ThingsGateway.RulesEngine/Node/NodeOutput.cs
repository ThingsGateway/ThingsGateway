using Newtonsoft.Json.Linq;

namespace ThingsGateway.RulesEngine;

public class NodeOutput
{
    private object output;
    public JToken JToken
    {
        get
        {
            return JToken.FromObject(output); ;
        }
    }

    public object Value
    {
        get
        {
            return output;
        }
        set
        {
            output = value;
        }
    }
}