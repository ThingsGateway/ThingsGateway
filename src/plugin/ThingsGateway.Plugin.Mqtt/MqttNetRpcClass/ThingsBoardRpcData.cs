//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Newtonsoft.Json;

namespace ThingsGateway.Plugin.Mqtt;

public class ThingsBoardRpcData
{
    public string device { get; set; }

    public Data data { get; set; }
}
public class Data
{
    public int id { get; set; }

    public string method { get; set; }

    [JsonProperty("params")]
    public Dictionary<string, string> @params { get; set; }
}

public class ThingsBoardRpcResponseData
{
    public string device { get; set; }

    public int id { get; set; }
    public ResponseData data { get; set; } = new();
}
public class ResponseData
{
    public bool success { get; set; }

    public string message { get; set; }

}
