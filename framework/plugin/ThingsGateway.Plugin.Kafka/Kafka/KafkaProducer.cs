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

using Confluent.Kafka;

using LiteDB;

using Microsoft.Extensions.Logging;

using System.Runtime.InteropServices;

using ThingsGateway.Foundation.Extension.ConcurrentQueue;
using ThingsGateway.Foundation.Extension.Generic;
using ThingsGateway.Foundation.Sockets;

namespace ThingsGateway.Plugin.Kafka;

/// <summary>
/// Kafka消息生产
/// </summary>
public partial class KafkaProducer : UpLoadBaseWithCacheT<DeviceData, VariableData>
{
    /// <inheritdoc/>
    public override Type DriverDebugUIType => null;

    public override Type DriverUIType => null;

    /// <inheritdoc/>
    public override VariablePropertyBase VariablePropertys => _variablePropertys;

    protected override IReadWrite _readWrite => null;

    protected override UploadPropertyWithCacheT _uploadPropertyWithCache => _driverPropertys;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    public override bool IsConnected() => success;

    /// <inheritdoc/>
    public override string ToString()
    {
        return $" {nameof(KafkaProducer)} {_driverPropertys.BootStrapServers} ";
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        producer?.Dispose();
        _collectDeviceRunTimes.Clear();
        _collectVariableRunTimes.Clear();
        base.Dispose(disposing);
    }

    private volatile bool producerSuccess = true;

    protected override void Init(ISenderClient client = null)
    {
        base.Init(client);

        #region Kafka 生产者

        //1、生产者配置
        producerconfig = new ProducerConfig
        {
            BootstrapServers = _driverPropertys.BootStrapServers,
            SecurityProtocol = _driverPropertys.SecurityProtocol,
            SaslMechanism = _driverPropertys.SaslMechanism,
        };
        if (!string.IsNullOrEmpty(_driverPropertys.SaslUsername))
            producerconfig.SaslUsername = _driverPropertys.SaslUsername;
        if (!string.IsNullOrEmpty(_driverPropertys.SaslPassword))
            producerconfig.SaslPassword = _driverPropertys.SaslPassword;

        //2、创建生产者
        producerBuilder = new ProducerBuilder<Null, string>(producerconfig);
        //3、错误日志监视
        producerBuilder.SetErrorHandler((p, msg) =>
        {
            if (producerSuccess)
                LogMessage?.LogWarning(msg.Reason);
            producerSuccess = !msg.IsError;
        });
        //kafka
        try
        {
            producer = producerBuilder.Build();
        }
        catch (DllNotFoundException)
        {
            if (!Library.IsLoaded)
            {
                string fileEx = ".dll";
                string osStr = "win-";
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    osStr = "win-";
                    fileEx = ".dll";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    osStr = "linux-";
                    fileEx = ".so";
                }
                else
                {
                    osStr = "osx-";
                    fileEx = ".dylib";
                }
                osStr += RuntimeInformation.ProcessArchitecture.ToString().ToLower();

                var pathToLibrd = System.IO.Path.Combine(Directory, "runtimes", osStr, "native", $"librdkafka{fileEx}");
                try
                {
                    Library.Load(pathToLibrd);
                }
                catch (Exception ex)
                {
                    LogMessage.LogError(ex, $"加载dll失败：{pathToLibrd}");
                }
            }
            producer = producerBuilder.Build();
        }

        #endregion
    }

    protected override async Task ProtectedExecuteAsync(CancellationToken cancellationToken)
    {
        var cacheItems = new List<CacheItem>();

        try
        {
            var list = _collectVariableRunTimes.ToListWithDequeue();
            if (list?.Count != 0)
            {
                var data = list.ChunkBetter(_driverPropertys.SplitSize);
                foreach (var item in data)
                {
                    try
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            string json = item.GetSciptListValue(_driverPropertys.BigTextScriptVariableModel);
                            var result = await KafKaUp($"{_driverPropertys.VariableTopic}", json, cancellationToken);
                            if (success != result.IsSuccess)
                            {
                                if (!result.IsSuccess)
                                    LogMessage.Warning(result.ToString());
                                success = result.IsSuccess;
                            }
                            if (!result.IsSuccess)
                            {
                                AddVarCahce(cacheItems, $"{_driverPropertys.VariableTopic}", json);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogMessage?.LogWarning(ex);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex);
        }

        try
        {
            var list = _collectDeviceRunTimes.ToListWithDequeue();
            if (list?.Count != 0)
            {
                var data = list.ChunkBetter(_driverPropertys.SplitSize);
                foreach (var item in data)
                {
                    try
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            string json = item.GetSciptListValue(_driverPropertys.BigTextScriptDeviceModel);
                            var result = await KafKaUp($"{_driverPropertys.DeviceTopic}", json, cancellationToken);
                            if (success != result.IsSuccess)
                            {
                                if (!result.IsSuccess)
                                    LogMessage.Warning(result.ToString());
                                success = result.IsSuccess;
                            }
                            if (!result.IsSuccess)
                            {
                                AddDevCache(cacheItems, $"{_driverPropertys.DeviceTopic}", json);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogMessage?.LogWarning(ex);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex);
        }

        if (cacheItems.Count > 0)
            CacheDb.Cache.Insert(cacheItems);

        List<long> successIds = new();
        try
        {
            var varList = CacheDb.Cache.Find(a => a.Type == varType, 0, 100).ToList();
            {
                foreach (var item in varList)
                {
                    try
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            var result = await KafKaUp($"{_driverPropertys.VariableTopic}", item.Value, cancellationToken);
                            if (success != result.IsSuccess)
                            {
                                if (!result.IsSuccess)
                                    LogMessage.Warning(result.ToString());
                                success = result.IsSuccess;
                            }
                            if (result.IsSuccess)
                                successIds.Add(item.Id);
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogMessage?.LogWarning(ex);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex);
        }

        try
        {
            var devList = CacheDb.Cache.Find(a => a.Type == devType, 0, 100).ToList();
            {
                foreach (var item in devList)
                {
                    try
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            var result = await KafKaUp($"{_driverPropertys.DeviceTopic}", item.Value, cancellationToken);
                            if (success != result.IsSuccess)
                            {
                                if (!result.IsSuccess)
                                    LogMessage.Warning(result.ToString());
                                success = result.IsSuccess;
                            }
                            if (result.IsSuccess)
                                successIds.Add(item.Id);
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogMessage?.LogWarning(ex);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex);
        }
        if (successIds.Count > 0)
            CacheDb.Cache.DeleteMany(a => successIds.Contains(a.Id));

        await Delay(_driverPropertys.CycleInterval, cancellationToken);
    }
}