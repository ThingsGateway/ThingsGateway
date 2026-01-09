//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Gateway.Application;

public static class GatewayExportString
{
    /// <summary>
    /// 通道名称
    /// </summary>
    public static string ChannelName => Localizer["ChannelName"];

    /// <summary>
    /// 设备名称
    /// </summary>
    public static string DeviceName => Localizer["DeviceName"];
    /// <summary>
    /// 设备名称
    /// </summary>
    public static string BusinessDeviceName => Localizer["BusinessDeviceName"];
    /// <summary>
    /// 冗余设备名称
    /// </summary>
    public static string RedundantDeviceName => Localizer["RedundantDeviceName"];

    /// <summary>
    /// 变量表名称
    /// </summary>
    public static string VariableName => Localizer["VariableName"];

    /// <summary>
    /// 变量报警表名称
    /// </summary>
    public static string AlarmName => Localizer["AlarmName"];

    public static IStringLocalizer localizer;
    public static IStringLocalizer Localizer
    {
        get
        {
            if (localizer == null)
                localizer = App.CreateLocalizerByType(typeof(GatewayExportString));
            return localizer;
        }
    }
}
