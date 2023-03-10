using Opc.Ua;

namespace ThingsGateway.Foundation.Adapter.OPCUA;
/// <summary>
/// 读取属性过程中用于描述的
/// </summary>
public class OPCNodeAttribute
{

    /// <summary>
    /// 属性的名称
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 操作结果状态描述
    /// </summary>
    public StatusCode StatusCode { get; set; }

    /// <summary>
    /// 属性的类型描述
    /// </summary>
    public string Type { get; set; }
    /// <summary>
    /// 属性的值，如果读取错误，返回文本描述
    /// </summary>
    public object Value { get; set; }

}

/// <summary>
/// OPC UA的状态更新消息
/// </summary>
public class OPCUAStatusEventArgs
{
    /// <summary>
    /// 是否异常
    /// </summary>
    public bool Error { get; set; }
    /// <summary>
    /// 文本
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// 时间
    /// </summary>
    public DateTime Time { get; set; }
    /// <summary>
    /// 转化为字符串
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return Error ? "[异常]" : "[正常]" + Time.ToString("  yyyy-MM-dd HH:mm:ss  ") + Text;
    }


}
