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
/// VariableSourceClass
/// </summary>
public class VariableSourceClass : IVariableSource
{
    private List<IVariable> _variableRunTimes = new();

    /// <inheritdoc/>
    public string? LastErrorMessage { get; set; }

    /// <inheritdoc/>
    public int Length { get; set; }

    /// <inheritdoc/>
    public string RegisterAddress { get; set; }

    /// <inheritdoc/>
    public TimeTick TimeTick { get; set; }

    /// <summary>
    /// 已打包变量
    /// </summary>
    public IEnumerable<IVariable> VariableRunTimes => _variableRunTimes;

    /// <inheritdoc/>
    public virtual void AddVariable(IVariable variable)
    {
        variable.VariableSource = this;
        _variableRunTimes.Add(variable);
    }

    /// <inheritdoc/>
    public virtual void AddVariableRange(IEnumerable<IVariable> variables)
    {
        foreach (var variable in variables)
        {
            variable.VariableSource = this;
        }
        _variableRunTimes.AddRange(variables);
    }
}
