using Mapster;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NewLife.Serialization;

using RabbitMQ.Client;

using System.Collections.Concurrent;
using System.Text;

using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Extension;
using ThingsGateway.Web.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.RabbitMQ
{
    public class RabbitMQClient : UpLoadBase
    {

        public RabbitMQClient(IServiceScopeFactory scopeFactory) : base(scopeFactory)
        {
        }
        public override OperResult Success()
        {
            if (_connection?.IsOpen == true)
            {

                return OperResult.CreateSuccessResult();
            }
            else
            {
                return new OperResult();
            }
        }

        [DeviceProperty("账号", "")] public string UserName { get; set; } = "guest";
        [DeviceProperty("密码", "")] public string Password { get; set; } = "guest";
        [DeviceProperty("IP", "")] public string IP { get; set; } = "localhost";
        [DeviceProperty("端口", "")] public int Port { get; set; } = 5672;
        [DeviceProperty("虚拟Host", "")] public string VirtualHost { get; set; } = ConnectionFactory.DefaultVHost;
        [DeviceProperty("路由名称", "")] public string RoutingKey { get; set; } = "TG";
        //[DeviceProperty("交换机名称", "")] public string ExchangeName { get; set; } = "RM";
        [DeviceProperty("变量队列名称", "")] public string VariableQueueName { get; set; } = "ThingsGateway/Variable";
        [DeviceProperty("设备队列名称", "")] public string DeviceQueueName { get; set; } = "ThingsGateway/Device";
        [DeviceProperty("是否发布List", "")] public bool IsList { get; set; } = false;
        [DeviceProperty("是否声明队列", "")] public bool IsQueueDeclare { get; set; } = false;
        [DeviceProperty("循环间隔", "最小500ms")] public int CycleInterval { get; set; } = 1000;

        public string ExchangeName { get; set; } = "";


        public override async Task BeforStart()
        {
            await Task.CompletedTask;
        }
        public override void Dispose()
        {
            _globalCollectDeviceData?.CollectVariables.ForEach(a => a.VariableValueChange -= VariableValueChange);

            _globalCollectDeviceData?.CollectDevices?.ForEach(a =>
            {
                a.DeviceStatusCahnge -= DeviceStatusCahnge;
            });
            _model?.Dispose();
            _connection?.Dispose();
        }
        private IModel _model;
        private IConnection _connection;
        private UploadDevice _curDevice { get; set; }
        RpcCore _rpcCore { get; set; }
        private ConnectionFactory _connectionFactory;
        protected override void Init(UploadDevice device)
        {
            _curDevice = device;
            _connectionFactory = new ConnectionFactory
            {
                HostName = IP,
                Port = Port,
                UserName = UserName,
                Password = Password,
                VirtualHost = VirtualHost,
            };



            using var serviceScope = _scopeFactory.CreateScope();
            _globalCollectDeviceData = serviceScope.ServiceProvider.GetService<GlobalCollectDeviceData>();
            _rpcCore = serviceScope.ServiceProvider.GetService<RpcCore>();

            _globalCollectDeviceData.CollectDevices.ForEach(a =>
            {
                a.DeviceStatusCahnge += DeviceStatusCahnge;
                DeviceStatusCahnge(a);
            });

            _globalCollectDeviceData.CollectVariables.ForEach(a =>
            {
                a.VariableValueChange += VariableValueChange;
                VariableValueChange(a);
            });


        }


        public override string ToString()
        {
            return $" {nameof(RabbitMQClient)} IP:{IP} Port:{Port}";
        }


        private GlobalCollectDeviceData _globalCollectDeviceData;

        private ConcurrentQueue<VariableData> CollectVariableRunTimes { get; set; } = new();
        private ConcurrentQueue<DeviceData> CollectDeviceRunTimes { get; set; } = new();

        private void DeviceStatusCahnge(CollectDeviceRunTime collectDeviceRunTime)
        {
            CollectDeviceRunTimes.Enqueue(collectDeviceRunTime.Adapt<DeviceData>());
        }

        private void VariableValueChange(CollectVariableRunTime collectVariableRunTime)
        {
            CollectVariableRunTimes.Enqueue(collectVariableRunTime.Adapt<VariableData>());
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
                        if (IsQueueDeclare)
                        {
                            _model?.QueueDeclare(VariableQueueName, true, false, false);
                            _model?.QueueDeclare(DeviceQueueName, true, false, false);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, ToString());
                    }
                }



                ////变化推送
                var varList = CollectVariableRunTimes.ToListWithDequeue();
                if (varList?.Count != 0)
                {
                    if (IsList)
                    {
                        var listChunk = varList.ChunkTrivialBetter(500);
                        foreach (var variables in listChunk)
                        {
                            try
                            {
                                var data = Encoding.UTF8.GetBytes(variables.ToJson());
                                // 设置消息持久化
                                IBasicProperties properties = _model?.CreateBasicProperties();
                                properties.Persistent = true;
                                _model?.BasicPublish(ExchangeName, VariableQueueName, properties, data);
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
                                var data = Encoding.UTF8.GetBytes(variable.ToJson());
                                // 设置消息持久化
                                IBasicProperties properties = _model?.CreateBasicProperties();
                                properties.Persistent = true;
                                _model?.BasicPublish(ExchangeName, VariableQueueName, properties, data);
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
                var devList = CollectDeviceRunTimes.ToListWithDequeue();
                if (devList?.Count != 0)
                {
                    if (IsList)
                    {
                        var listChunk = devList.ChunkTrivialBetter(500);
                        foreach (var devices in listChunk)
                        {
                            try
                            {
                                var data = Encoding.UTF8.GetBytes(devices.ToJson());
                                // 设置消息持久化
                                IBasicProperties properties = _model?.CreateBasicProperties();
                                properties.Persistent = true;
                                _model?.BasicPublish(ExchangeName, DeviceQueueName, properties, data);
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
                                var data = Encoding.UTF8.GetBytes(devices.ToJson());
                                // 设置消息持久化
                                IBasicProperties properties = _model?.CreateBasicProperties();
                                properties.Persistent = true;
                                _model?.BasicPublish(ExchangeName, DeviceQueueName, properties, data);
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

            if (CycleInterval > 500 + 50)
            {
                await Task.Delay(CycleInterval - 500);
            }
            else
            {

            }

        }
    }


}
