// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://kimdiego2098.github.io/
// QQ群：605534569
// ------------------------------------------------------------------------------

using Newtonsoft.Json.Linq;

using System.Collections.Concurrent;

using ThingsGateway.Core.Extension;
using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Plugin.VariableExpression;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class MemoryVariable : CollectBase
{
    private readonly MemoryVariableProperty _driverPropertys = new();

    /// <inheritdoc/>
    public override CollectPropertyBase CollectProperties => _driverPropertys;

    public override Type DriverUIType => null;

    /// <inheritdoc/>
    public override IProtocol Protocol => null;

    private volatile bool success = true;

    protected override async ValueTask<OperResult<byte[]>> ReadSourceAsync(VariableSourceRead variableSourceRead, CancellationToken cancellationToken)
    {
        if (IsSingleThread)
        {
            while (WriteLock.IsWaitting)
            {
                await Task.Delay(100).ConfigureAwait(false);
            }
        }

        if (cancellationToken.IsCancellationRequested)
            return new(new OperationCanceledException());
        Interlocked.Increment(ref variableSourceRead.ReadCount);
        OperResult operResult = new();
        var time = DateTime.Now;
        foreach (var property in variableSourceRead.VariableRunTimes)
        {
            if (property.Value == null)
            {
                var result = property.SetValue(property.Value.ChangeType(property.DataType.GetSystemType()), time);
                if (!result.IsSuccess)
                {
                    variableSourceRead.LastErrorMessage = result.ErrorMessage;
                    operResult = result;
                    success = false;
                }
            }
        }
        if (operResult.IsSuccess)
            success = true;
        return new OperResult<byte[]>(operResult);
    }

    public override bool IsConnected() => success;

    protected override string GetAddressDescription()
    {
        return string.Empty;
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

    protected override void Init(IChannel? channel = null)
    {
    }

    protected override async ValueTask<Dictionary<string, OperResult>> WriteValuesAsync(Dictionary<VariableRunTime, JToken> writeInfoLists, CancellationToken cancellationToken)
    {
        try
        {
            // 如果是单线程模式，则等待写入锁
            if (IsSingleThread)
                await WriteLock.WaitAsync(cancellationToken).ConfigureAwait(false);

            // 创建用于存储操作结果的并发字典
            ConcurrentDictionary<string, OperResult> operResults = new();
            var time = DateTime.Now;

            // 使用并发方式遍历写入信息列表，并进行异步写入操作
            writeInfoLists.ParallelForEach((writeInfo) =>
           {
               try
               {
                   object value;
                   if (writeInfo.Value is JValue jValue)
                   {
                       value = jValue.Value;
                   }
                   else
                   {
                       value = writeInfo.Value;
                   }

                   writeInfo.Key.SetValue(value, time);

                   // 将操作结果添加到结果字典中，使用变量名称作为键
                   operResults.TryAdd(writeInfo.Key.Name, new OperResult());
               }
               catch (Exception ex)
               {
                   operResults.TryAdd(writeInfo.Key.Name, new(ex));
               }
           });

            // 返回包含操作结果的字典
            return new Dictionary<string, OperResult>(operResults);
        }
        finally
        {
            // 如果是单线程模式，则释放写入锁
            if (IsSingleThread)
                WriteLock.Release();
        }
    }
}
