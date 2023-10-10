#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

namespace ThingsGateway.Core;
/// <summary>
/// DmtpTypeEnum
/// </summary>
public enum DmtpTypeEnum
{
    /// <summary>
    /// File
    /// </summary>
    File,
    /// <summary>
    /// GatewayDB
    /// </summary>
    GatewayDB,
}

/// <summary>
/// FilePluginHelpers
/// </summary>
public static class FilePluginUtil
{
    /// <summary>
    /// DmtpType
    /// </summary>
    public const string DmtpType = "DmtpType";
    /// <summary>
    /// FileName
    /// </summary>
    public const string FileName = "FileName";
    /// <summary>
    /// GetFileTempPath
    /// </summary>
    /// <param name="client"></param>
    /// <returns></returns>
    public static string GetFileTempPath(TcpClientBase client)
    {
        return $"{AppContext.BaseDirectory}FileTemp/{client.RemoteIPHost.ToString().Replace(":", "-").Replace(@"/", @"-")}";
    }
}