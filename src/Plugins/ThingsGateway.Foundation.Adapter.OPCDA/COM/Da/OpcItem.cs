﻿#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using ThingsGateway.Foundation.Adapter.OPCDA.Rcw;

namespace ThingsGateway.Foundation.Adapter.OPCDA.Da;

public class OpcItem
{
    private static int _hanle = 0;
    public OpcItem(string itemId)
    {
        ItemID = itemId;
        ClientHandle = ++_hanle;
    }

    public string AccessPath { get; private set; } = "";

    public IntPtr Blob { get; set; } = IntPtr.Zero;

    public int BlobSize { get; set; } = 0;

    public int ClientHandle { get; private set; }

    /// <summary>
    /// active(1) or not(0)
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 数据项在opc server的完全名称
    /// </summary>
    public string ItemID { get; private set; } = String.Empty;

    public int Quality { get; set; } = Qualities.OPC_QUALITY_BAD;
    public short RunTimeDataType { get; set; } = 0;
    public int ServerHandle { get; set; }
    public DateTime TimeStamp { get; set; } = new DateTime(0);
    public object Value { get; set; }
}
