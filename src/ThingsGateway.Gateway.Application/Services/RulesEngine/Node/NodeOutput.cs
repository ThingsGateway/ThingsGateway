using System.Text.Json.Nodes;

namespace ThingsGateway.Gateway.Application;

public class NodeOutput
{
    private object output;
    public JsonNode JToken
    {
        get
        {
            return JsonUtil.GetJsonNodeFromObj(output);
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