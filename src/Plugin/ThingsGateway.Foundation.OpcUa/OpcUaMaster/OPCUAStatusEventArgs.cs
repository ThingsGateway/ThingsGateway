//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation.OpcUa;

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
