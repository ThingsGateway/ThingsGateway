using System.Text.Json.Nodes;

namespace ThingsGateway.Gateway.Application;

public class NodeInput
{
    private object input;
    public JsonNode JToken
    {
        get
        {
            return JsonUtil.GetJsonNodeFromObj(input);
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