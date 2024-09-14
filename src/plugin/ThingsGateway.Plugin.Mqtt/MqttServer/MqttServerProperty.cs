//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Plugin.Mqtt;
public class Prev
{
    /// <summary>
    /// 
    /// </summary>
    public string penId { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public double x { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public double y { get; set; }
}

public class AnchorsItem
{
    /// <summary>
    /// 
    /// </summary>
    public string id { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public double lineLength { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string penId { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public Prev prev { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int prevNextType { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string start { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int x { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int y { get; set; }
}

public class Where
{
    /// <summary>
    /// 
    /// </summary>
    public string comparison { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string key { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string type { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int value { get; set; }
}

public class EventsItem
{
    /// <summary>
    /// 
    /// </summary>
    public int action { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string name { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public Where where { get; set; }
}

public class PensItem
{
    /// <summary>
    /// 
    /// </summary>
    public List<AnchorsItem> anchors { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string animateColor { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string animateCycle { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string animateReverse { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int animateSpan { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string autoPlay { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string autoPolyline { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string borderColor { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int borderWidth { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public Center center { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public List<string> children { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string color { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public List<EventsItem> events { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public double ex { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public double ey { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int fontSize { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public List<string> form { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int globalAlpha { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public double height { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string hiddenText { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string id { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string keepAnimateState { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public double length { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int lineAnimateType { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public double lineHeight { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string lineName { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int lineWidth { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int locked { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string name { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int rotate { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int shadowBlur { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int shadowOffsetX { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public List<string> tags { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string text { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int type { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string value { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string visible { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public double width { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public double x { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public double y { get; set; }
}

public class Origin
{
    /// <summary>
    /// 
    /// </summary>
    public double x { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public double y { get; set; }
}

public class Center
{
    /// <summary>
    /// 
    /// </summary>
    public int x { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int y { get; set; }
}

public class Paths
{
}

public class HttpsItem
{
    /// <summary>
    /// 
    /// </summary>
    public string http { get; set; }
}

public class Root
{
    /// <summary>
    /// 
    /// </summary>
    public int x { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int y { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public double scale { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public List<PensItem> pens { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public Origin origin { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public Center center { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public Paths paths { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string template { get; set; }
    /// <summary>
    /// 手阀
    /// </summary>
    public string name { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string shared { get; set; }
    /// <summary>
    /// 智慧园区
    /// </summary>
    public string @case { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string system { get; set; }
    /// <summary>
    /// 物联网
    /// </summary>
    public string folder { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int view { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int star { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int favorite { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string ownerId { get; set; }
    /// <summary>
    /// 林冠旭
    /// </summary>
    public string ownerName { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string editorId { get; set; }
    /// <summary>
    /// 乐乐
    /// </summary>
    public string editorName { get; set; }
    /// <summary>
    /// '处':3 '处理':6 '废':1 '废水':5 '水':2 '理':4
    /// </summary>
    public string search_vector { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string createdAt { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string updatedAt { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string autoAlignGrid { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string autoSizeinMobile { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string autoSizeinPc { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string background { get; set; }
    /// <summary>
    /// 水利水务
    /// </summary>
    public string @class { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string component { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public List<string> dataPoints { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string desc { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public List<string> groups { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string http { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public List<HttpsItem> https { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string localSaveAt { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int locked { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string mqttOptions { get; set; }
    /// <summary>
    /// 废水处理
    /// </summary>
    public string name1 { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string previewUnScale { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int price { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string src { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string tags { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string type { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string version { get; set; }
}

/// <summary>
/// <inheritdoc/>
/// </summary>
public class MqttServerProperty : BusinessPropertyWithCacheIntervalScript
{
    /// <summary>
    /// IP
    /// </summary>
    [DynamicProperty]
    public string IP { get; set; } = "127.0.0.1";

    /// <summary>
    /// 端口
    /// </summary>
    [DynamicProperty]
    public int Port { get; set; } = 1883;

    /// <summary>
    /// WebSocket端口
    /// </summary>
    [DynamicProperty]
    public int WebSocketPort { get; set; } = 8083;

    /// <summary>
    /// 允许连接的ID(前缀)
    /// </summary>
    [DynamicProperty]
    public string StartWithId { get; set; } = "ThingsGatewayId";

    /// <summary>
    /// 允许Rpc写入
    /// </summary>
    [DynamicProperty]
    public bool DeviceRpcEnable { get; set; }


    /// <summary>
    /// Rpc写入Topic
    /// </summary>
    [DynamicProperty(Remark = "实际的写入主题为固定通配 {RpcWrite/+} ，其中RpcWrite为该属性填入内容，+通配符是请求GUID值；返回结果主题会在主题后添加Response , 也就是{RpcWrite/+/Response}")]
    public string RpcWriteTopic { get; set; }
}
