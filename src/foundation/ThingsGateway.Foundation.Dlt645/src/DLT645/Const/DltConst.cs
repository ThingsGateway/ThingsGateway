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

namespace ThingsGateway.Foundation.Dlt645;

/// <summary>
/// 常量
/// </summary>
public class DltConst
{
    /// <summary>
    /// 地址说明
    /// </summary>
    public const string AddressDes =
"""
查看附带文档或者相关资料，下面列举一下常见的数据标识地址

地址                       说明
-----------------------------------------
02010100    A相电压
02020100    A相电流
02030000    瞬时总有功功率
00000000    (当前)组合有功总电能
00010000    (当前)正向有功总电能
"""
;

    /// <summary>
    /// Dtu-{0}-已连接
    /// </summary>
    public const string DtuConnected = "Dtu-{0}-Connected.";

    /// <summary>
    /// 客户端未连接，或寄存器设置错误，必须设置ID={DTU注册包}
    /// </summary>
    public const string DtuNoConnectedWaining = "The client is not connected or the register is set incorrectly. id={Dtu registration package} must be set.";
}