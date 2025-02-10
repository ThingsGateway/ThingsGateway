using Newtonsoft.Json.Linq;

namespace ThingsGateway.RulesEngine;

public class NodeInput
{
    private object input;
    public JToken JToken
    {
        get
        {
            return JToken.FromObject(input); ;
        }
    }

    public object Value
    {
        get
        {
            return input;
        }
        set
        {
            input = value;
        }
    }
}