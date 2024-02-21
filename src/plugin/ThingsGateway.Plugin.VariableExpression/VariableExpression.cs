//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Plugin.Modbus;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class VariableExpression : CollectBase
{
    private readonly VariableExpressionProperty _driverPropertys = new();

    /// <inheritdoc/>
    public override CollectPropertyBase DriverPropertys => _driverPropertys;

    public override Type DriverUIType => null;

    /// <inheritdoc/>
    protected override IProtocol Protocol => null;

    private volatile bool success = true;

    /// <summary>
    /// 支持自增值
    /// </summary>
    protected override Task<OperResult<byte[]>> ReadSourceAsync(VariableSourceRead variableSourceRead, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref variableSourceRead.ReadCount);
        success = true;
        OperResult? operResult = new();
        foreach (var property in variableSourceRead.VariableRunTimes)
        {
            var result = property.SetValue(variableSourceRead.ReadCount);
            if (!result.IsSuccess)
            {
                variableSourceRead.LastErrorMessage = result.ErrorMessage;
                operResult = result;
                success = false;
            }
        }
        return Task.FromResult(new OperResult<byte[]>(operResult));
    }

    public override bool IsConnected() => success;

    private const string addressConst = "只支持读取表达式获取或自增，变量地址无意义";

    protected override string GetAddressDescription()
    {
        return addressConst;
    }

    /// <inheritdoc/>
    protected override List<VariableSourceRead> ProtectedLoadSourceRead(List<VariableRunTime> deviceVariables)
    {
        List<VariableSourceRead> variableSourceReads = new List<VariableSourceRead>();
        foreach (var item in deviceVariables)
        {
            VariableSourceRead variableSourceRead = new();
            variableSourceRead.RegisterAddress = item.RegisterAddress;
            variableSourceRead.TimeTick = new(item.IntervalTime ?? CurrentDevice.IntervalTime);
            variableSourceRead.AddVariable(item);
            variableSourceReads.Add(variableSourceRead);
        }
        return variableSourceReads;
    }
}