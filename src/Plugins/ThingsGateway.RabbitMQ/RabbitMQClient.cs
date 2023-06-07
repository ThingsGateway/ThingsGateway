#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using Mapster;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using RabbitMQ.Client;

using System.Collections.Concurrent;
using System.Text;

using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Extension;
using ThingsGateway.Web.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.RabbitMQ;
public class RabbitMQClient : UpLoadBase
{

    private ConcurrentQueue<DeviceData> _collectDeviceRunTimes = new();
    private ConcurrentQueue<VariableData> _collectVariableRunTimes = new();
    private IConnection _connection;

    private ConnectionFactory _connectionFactory;

    private GlobalCollectDeviceData _globalCollectDeviceData;

    private IModel _model;

    private RpcSingletonService _rpcCore;
    private List<CollectVariableRunTime> _uploadVariables = new();
    private RabbitMQClientProperty driverPropertys = new();

    private RabbitMQClientVariableProperty variablePropertys = new();

    public RabbitMQClient(IServiceScopeFactory scopeFactory) : base(scopeFactory)
    {
    }
    public override UpDriverPropertyBase DriverPropertys => driverPropertys;

    public string ExchangeName { get; set; } = "";

    public override List<CollectVariableRunTime> UploadVariables => _uploadVariables;
    public override VariablePropertyBase VariablePropertys => variablePropertys;
    public override async Task BeforStartAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
    public override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (_model == null)
            {
                try
                {

                    // 创建连接
                    if (_connection == null)
                        _connection = _connectionFactory.CreateConnection();
                    // 创建通道
                    if (_model == null)
                        _model = _connection.CreateModel();
                    // 声明路由队列
                    if (driverPropertys.IsQueueDeclare)
                    {
                        _model?.QueueDeclare(driverPropertys.VariableQueueName, true, false, false);
                        _model?.QueueDeclare(driverPropertys.DeviceQueueName, true, false, false);
                    }


                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, ToString());
                }
            }

            ////变化推送
            var varList = _collectVariableRunTimes.ToListWithDequeue();
            if (varList?.Count != 0)
            {
                if (driverPropertys.IsList)
                {
                    var listChunk = varList.ChunkTrivialBetter(500);
                    foreach (var variables in listChunk)
                    {
                        try
                        {
                            if (!cancellationToken.IsCancellationRequested)
                            {
                                var data = Encoding.UTF8.GetBytes(variables.GetSciptListValue(driverPropertys.BigTextScriptVariableModel));
                                // 设置消息持久化
                                IBasicProperties properties = _model?.CreateBasicProperties();
                                properties.Persistent = true;
                                _model?.BasicPublish(ExchangeName, driverPropertys.VariableQueueName, properties, data);
                            }
                            else
                            {
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, ToString());
                        }

                    }
                }
                else
                {
                    foreach (var variable in varList)
                    {
                        try
                        {
                            if (!cancellationToken.IsCancellationRequested)
                            {
                                var data = Encoding.UTF8.GetBytes(variable.GetSciptListValue(driverPropertys.BigTextScriptVariableModel));
                                // 设置消息持久化
                                IBasicProperties properties = _model?.CreateBasicProperties();
                                properties.Persistent = true;
                                _model?.BasicPublish(ExchangeName, driverPropertys.VariableQueueName, properties, data);
                            }
                            else
                            {
                                break;
                            }

                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, ToString());
                        }
                    }

                }

            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, ToString());
        }
        try
        {
            ////变化推送
            var devList = _collectDeviceRunTimes.ToListWithDequeue();
            if (devList?.Count != 0)
            {
                if (driverPropertys.IsList)
                {
                    var listChunk = devList.ChunkTrivialBetter(500);
                    foreach (var devices in listChunk)
                    {
                        try
                        {
                            var data = Encoding.UTF8.GetBytes(devices.GetSciptListValue(driverPropertys.BigTextScriptDeviceModel));
                            // 设置消息持久化
                            IBasicProperties properties = _model?.CreateBasicProperties();
                            properties.Persistent = true;
                            _model?.BasicPublish(ExchangeName, driverPropertys.DeviceQueueName, properties, data);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, ToString());
                        }

                    }
                }
                else
                {
                    foreach (var devices in devList)
                    {
                        try
                        {
                            var data = Encoding.UTF8.GetBytes(devices.GetSciptListValue(driverPropertys.BigTextScriptDeviceModel));
                            // 设置消息持久化
                            IBasicProperties properties = _model?.CreateBasicProperties();
                            properties.Persistent = true;
                            _model?.BasicPublish(ExchangeName, driverPropertys.DeviceQueueName, properties, data);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, ToString());
                        }
                    }

                }

            }

        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, ToString());
        }

        if (driverPropertys.CycleInterval > UploadDeviceThread.CycleInterval + 50)
        {
            try
            {
                await Task.Delay(driverPropertys.CycleInterval - UploadDeviceThread.CycleInterval, cancellationToken);
            }
            catch
            {
            }
        }
        else
        {

        }

    }

    public override OperResult IsConnected()
    {
        return _connection?.IsOpen == true ? OperResult.CreateSuccessResult() : new OperResult();
    }

    public override string ToString()
    {
        return $" {nameof(RabbitMQClient)} IP:{driverPropertys.IP} Port:{driverPropertys.Port}";
    }

    protected override void Dispose(bool disposing)
    {
        _globalCollectDeviceData?.CollectVariables.ForEach(a => a.VariableValueChange -= VariableValueChange);

        _globalCollectDeviceData?.CollectDevices?.ForEach(a =>
        {
            a.DeviceStatusCahnge -= DeviceStatusCahnge;
        });
        _model?.SafeDispose();
        _connection?.SafeDispose();
        _uploadVariables = null;
        _collectDeviceRunTimes.Clear();
        _collectVariableRunTimes.Clear();
        _collectDeviceRunTimes = null;
        _collectVariableRunTimes = null;
    }
    protected override void Init(UploadDeviceRunTime device)
    {
        _connectionFactory = new ConnectionFactory
        {
            HostName = driverPropertys.IP,
            Port = driverPropertys.Port,
            UserName = driverPropertys.UserName,
            Password = driverPropertys.Password,
            VirtualHost = driverPropertys.VirtualHost,
        };



        var serviceScope = _scopeFactory.CreateScope();
        _globalCollectDeviceData = serviceScope.ServiceProvider.GetService<GlobalCollectDeviceData>();
        _rpcCore = serviceScope.ServiceProvider.GetService<RpcSingletonService>();


        var tags = _globalCollectDeviceData.CollectVariables.Where(a => a.VariablePropertys.ContainsKey(device.Id))
       .Where(b => b.VariablePropertys[device.Id].Any(c =>
       {
           if (c.PropertyName == nameof(variablePropertys.Enable))
           {
               if (c.Value?.GetBoolValue() == true)
                   return true;
               else
                   return false;
           }
           else
               return false;
       }))
       .ToList();

        _uploadVariables = tags;

        _globalCollectDeviceData.CollectDevices.Where(a => _uploadVariables.Select(b => b.DeviceId).Contains(a.Id)).ForEach(a =>
        {
            a.DeviceStatusCahnge += DeviceStatusCahnge;
            DeviceStatusCahnge(a);
        });
        _uploadVariables.ForEach(a =>
        {
            a.VariableValueChange += VariableValueChange;
            VariableValueChange(a);
        });



    }
    private void DeviceStatusCahnge(CollectDeviceRunTime collectDeviceRunTime)
    {
        _collectDeviceRunTimes.Enqueue(collectDeviceRunTime.Adapt<DeviceData>());
    }

    private void VariableValueChange(CollectVariableRunTime collectVariableRunTime)
    {
        _collectVariableRunTimes.Enqueue(collectVariableRunTime.Adapt<VariableData>());
    }
}
