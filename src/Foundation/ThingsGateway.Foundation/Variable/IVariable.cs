//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation;

/// <summary>
/// IVariable
/// </summary>
public interface IVariable
{
    /// <summary>
    /// 数据类型
    /// </summary>
    DataTypeEnum DataType { get; set; }

    /// <summary>
    /// 偏移量，注意如果是布尔类型，Index应该为bit的偏移
    /// </summary>
    int Index { get; set; }

    /// <summary>
    /// 执行间隔
    /// </summary>
    int? IntervalTime { get; set; }

    /// <summary>
    /// 寄存器地址
    /// </summary>
    string? RegisterAddress { get; set; }

    /// <summary>
    /// 数据转换规则
    /// </summary>
    IThingsGatewayBitConverter ThingsGatewayBitConverter { get; set; }

    /// <summary>
    /// 实时值
    /// </summary>
    object? Value { get; }

    /// <summary>
    /// 打包变量
    /// </summary>
    IVariableSource VariableSource { get; set; }

    /// <summary>
    /// 赋值变量，返回是否成功，一般在实体内部需要做异常保存
    /// </summary>
    /// <param name="value">值,可能会是基础类型，也有可能是数组、JsonNode</param>
    /// <param name="dateTime">采集时间</param>
    /// <param name="isOnline">是否在线</param>
    /// <returns></returns>
    OperResult SetValue(object? value, DateTime dateTime, bool isOnline = true);
}
