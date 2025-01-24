//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Foundation.OpcDa.Rcw;

namespace ThingsGateway.Foundation.OpcDa.Da;

/// <summary>
/// OpcItem
/// </summary>
public class OpcItem
{
    private static int _hanle = 0;

    /// <summary>
    /// OpcItem
    /// </summary>
    /// <param name="itemId"></param>
    public OpcItem(string itemId)
    {
        ItemID = itemId;
        ClientHandle = ++_hanle;
    }

    /// <summary>
    /// AccessPath
    /// </summary>
    public string AccessPath { get; private set; } = "";

    /// <summary>
    /// Blob
    /// </summary>
    public IntPtr Blob { get; set; } = IntPtr.Zero;

    /// <summary>
    /// BlobSize
    /// </summary>
    public int BlobSize { get; set; } = 0;

    /// <summary>
    /// ClientHandle
    /// </summary>
    public int ClientHandle { get; private set; }

    /// <summary>
    /// active(1) or not(0)
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 数据项在opc server的完全名称
    /// </summary>
    public string ItemID { get; private set; } = String.Empty;

    /// <summary>
    /// Quality
    /// </summary>
    public int Quality { get; set; } = Qualities.OPC_QUALITY_BAD;

    /// <summary>
    /// RunTimeDataType
    /// </summary>
    public short RunTimeDataType { get; set; } = 0;

    /// <summary>
    /// ServerHandle
    /// </summary>
    public int ServerHandle { get; set; }

    /// <summary>
    /// TimeStamp
    /// </summary>
    public DateTime TimeStamp { get; set; } = new DateTime(0);

    /// <summary>
    /// Value
    /// </summary>
    public object Value { get; set; }
}