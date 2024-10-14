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
/// IVariableSource
/// </summary>
public interface IVariableSource
{
    /// <summary>
    /// 最后一次失败原因
    /// </summary>
    string? LastErrorMessage { get; set; }

    /// <summary>
    /// 长度
    /// </summary>
    int Length { get; set; }

    /// <summary>
    /// 变量地址
    /// </summary>
    string RegisterAddress { get; set; }

    /// <summary>
    /// TimeTick
    /// </summary>
    TimeTick TimeTick { get; set; }

    /// <summary>
    /// 添加变量
    /// </summary>
    void AddVariable(IVariable variable);

    /// <summary>
    /// 添加变量
    /// </summary>
    void AddVariableRange(IEnumerable<IVariable> variables);
}
