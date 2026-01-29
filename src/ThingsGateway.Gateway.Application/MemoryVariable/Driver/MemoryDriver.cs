//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;
using System.Collections.Concurrent;
using System.Text.Json.Nodes;
using ThingsGateway.Gateway.Application.Extensions;
using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;


/// <summary>
/// <inheritdoc/>
/// </summary>
[DisplayNonePlugin]
public class MemoryDriver : CollectBase
{
    /// <inheritdoc/>
    public override CollectPropertyBase CollectProperties { get; } = new CollectPropertyNone();


#if !Management
    protected internal override async Task InitChannelAsync(ChannelObject channelObject, CancellationToken cancellationToken)
    {
        GlobalData.VariableValueChangeEvent -= GlobalData_VariableValueChangeEvent;
        GlobalData.VariableValueChangeEvent += GlobalData_VariableValueChangeEvent;
        await base.InitChannelAsync(channelObject, cancellationToken).ConfigureAwait(false);
    }
    protected override Task DisposeAsync(bool disposing)
    {
        GlobalData.VariableValueChangeEvent -= GlobalData_VariableValueChangeEvent;
        return base.DisposeAsync(disposing);
    }
    private void GlobalData_VariableValueChangeEvent(VariableRuntime variableRuntime, VariableBasicData variableData)
    {
        if (ChangedVariableRuntimes.TryGetValue((variableRuntime.DeviceName, variableRuntime.Name), out var list))
        {
            DateTime now = DateTime.Now;
            foreach (var item in list)
            {
                try
                {
                    item.Value.SetValue(null, now, true);
                }
                catch (Exception ex)
                {
                    LogMessage?.LogError(ex, $"Memory Variable {item.Value.Name} Read Expression Error");
                }
            }
        }


    }

    protected override ValueTask<OperResult<ReadOnlyMemory<byte>>> ReadSourceAsync(VariableSourceRead deviceVariableSourceRead, CancellationToken cancellationToken)
    {
        DateTime now = DateTime.Now;
        OperResult operResult = default;
        foreach (var item in deviceVariableSourceRead.Variables)
        {
            var result = item.SetValue(null, now, true);
            if (result.IsSuccess == false)
            {
                operResult = result;
            }
        }
        if (operResult.IsSuccess == false)
        {
            return ValueTask.FromResult(new OperResult<ReadOnlyMemory<byte>>("One or more variables failed"));
        }
        return ValueTask.FromResult(new OperResult<ReadOnlyMemory<byte>>());
    }
    protected override ValueTask<Dictionary<string, OperResult>> WriteValuesAsync(Dictionary<VariableRuntime, JsonNode> writeInfoLists, CancellationToken cancellationToken)
    {
        Dictionary<string, OperResult> keyValuePairs = new();
        foreach (var item in writeInfoLists)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            if (item.Key is IMemoryVariableRpc memoryVariableRpc)
            {
                keyValuePairs.Add(item.Key.Name, memoryVariableRpc.MemoryVariableRpc(item.Value, cancellationToken));
            }
        }
        return ValueTask.FromResult(keyValuePairs);
    }
    public NonBlockingDictionary<(string, string), NonBlockingDictionary<long, MemoryVariableRuntime>> ChangedVariableRuntimes = new();
    protected override Task<List<VariableSourceRead>> ProtectedLoadSourceReadAsync(List<VariableRuntime> deviceVariables)
    {
        var memoryVars = deviceVariables.Cast<MemoryVariableRuntime>();
        List<VariableSourceRead> deviceVariableSourceReads = new();

#pragma warning disable CA1851 // “IEnumerable”集合可能的多个枚举
        var changeVars = memoryVars.Where(a => a.BusinessUpdate != BusinessUpdateEnum.Interval);
        var intervalVars = memoryVars.Where(a => a.BusinessUpdate != BusinessUpdateEnum.Change).ToList();
#pragma warning restore CA1851 // “IEnumerable”集合可能的多个枚举




        DateTime now = DateTime.Now;
        //位号变化触发的，转为字典，由全局变化事件检索
        foreach (var item in changeVars)
        {
            try
            {
                var exo = MemoryExpressionEvaluatorExtension.GetMemoryReadWriteExpressions(item.ReadExpressions);
                _ = exo.GetNewValue();
                foreach (var devVars in exo.Tags)
                {
                    if (ChangedVariableRuntimes.TryGetValue(devVars, out var list))
                    {
                        list.AddOrUpdate(item.Id, (a) => item, (a, b) => item);
                    }
                    else
                    {
                        ChangedVariableRuntimes[devVars] = new NonBlockingDictionary<long, MemoryVariableRuntime>();
                        ChangedVariableRuntimes[devVars].TryAdd(item.Id, item);
                    }

                    if (GlobalData.TryGetVariable(devVars.Item1, devVars.Item2, out var variable))
                    {
                        try
                        {
                            variable.SetValue(null, now, true);
                        }
                        catch (Exception ex)
                        {
                            LogMessage?.LogError(ex, $"Memory Variable {item.Name} Read Expression Error");
                        }
                    }
                }

                if (exo.Tags.Count == 0)
                {
                    intervalVars.Add(item);
                }
            }
            catch (Exception ex)
            {
                LogMessage?.LogError(ex, $"Memory Variable {item.Name} Read Expression Error");
            }
        }

        var groupedIntervalVars = intervalVars.GroupBy(a => a.IntervalTime ?? CurrentDevice.IntervalTime);
        foreach (var variableRuntimeKey in groupedIntervalVars)
        {
            var deviceVariableSourceRead = new VariableSourceRead();
            deviceVariableSourceRead.IntervalTime = new(variableRuntimeKey.Key);
            deviceVariableSourceRead.AddVariableRange(variableRuntimeKey);
            deviceVariableSourceReads.Add(deviceVariableSourceRead);
        }
        return Task.FromResult(deviceVariableSourceReads);
    }


    public override bool IsConnected() => true;


#endif
}
