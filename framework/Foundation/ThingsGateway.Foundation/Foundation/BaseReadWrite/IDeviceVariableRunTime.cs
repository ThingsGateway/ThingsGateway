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

namespace ThingsGateway.Foundation;

/// <summary>
/// 打包读取变量
/// </summary>
public interface IDeviceVariableRunTime
{
    /// <summary>
    /// 执行间隔
    /// </summary>
    int IntervalTime { get; set; }
    /// <summary>
    /// 变量地址
    /// </summary>
    string VariableAddress { get; set; }
    /// <summary>
    /// Index
    /// </summary>
    int Index { get; set; }
    /// <summary>
    /// 赋值
    /// </summary>
    OperResult SetValue(object value, DateTime dateTime = default, bool isOnline = true);
    /// <summary>
    /// IThingsGatewayBitConverter
    /// </summary>
    IThingsGatewayBitConverter ThingsGatewayBitConverter { get; set; }
    /// <summary>
    /// 数据类型
    /// </summary>
    DataTypeEnum DataTypeEnum { get; set; }
}