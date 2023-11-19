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

using RabbitMQ.Client;

using System.Text;

using Yitter.IdGenerator;

namespace ThingsGateway.Plugin.RabbitMQ;

/// <summary>
/// MqttClient
/// </summary>
public partial class RabbitMQProducer : UpLoadBaseWithCacheT<DeviceData, VariableData>
{
    private const string devType = "dev";
    private const string varType = "var";
    private readonly RabbitMQProducerProperty _driverPropertys = new();
    private readonly RabbitMQProducerVariableProperty _variablePropertys = new();
    private readonly EasyLock easyLock = new();
    private IConnection _connection;
    private ConnectionFactory _connectionFactory;
    private IModel _model;
    private volatile bool success = true;


    protected override void AddCache(List<CacheItem> cacheItems, IEnumerable<VariableData> dev)
    {
        AddVarCahce(cacheItems, $"{_driverPropertys.VariableTopic}", dev.GetSciptListValue(_driverPropertys.BigTextScriptVariableModel));
    }

    private void AddVarCahce(List<CacheItem> cacheItems, string key, string data)
    {
        var cacheItem = new CacheItem()
        {
            Id = YitIdHelper.NextId(),
            Key = key,
            Type = varType,
            Value = data
        };
        cacheItems.Add(cacheItem);
    }

    protected override void AddCache(List<CacheItem> cacheItems, IEnumerable<DeviceData> dev)
    {
        AddDevCache(cacheItems, $"{_driverPropertys.DeviceTopic}", dev.GetSciptListValue(_driverPropertys.BigTextScriptDeviceModel));
    }

    private void AddDevCache(List<CacheItem> cacheItems, string key, string data)
    {
        var cacheItem = new CacheItem()
        {
            Id = YitIdHelper.NextId(),
            Key = key,
            Type = devType,
            Value = data
        };
        cacheItems.Add(cacheItem);
    }


    /// <summary>
    /// rabbitMQ上传，返回上传结果
    /// </summary>
    private OperResult Publish(string topic, string payLoad, IBasicProperties properties)
    {
        try
        {
            if (properties != null)
                properties.Persistent = true;
            if (_model != null)
            {
                _model.BasicPublish(_driverPropertys.ExchangeName, topic, properties, Encoding.UTF8.GetBytes(payLoad));
                CurrentDevice.SetDeviceStatus(DateTimeExtensions.CurrentDateTime, 0);
                LogMessage.Trace($"{FoundationConst.LogMessageHeader}主题：{topic}{Environment.NewLine}负载：{payLoad}");
                return OperResult.CreateSuccessResult();
            }
            else
            {
                CurrentDevice.SetDeviceStatus(DateTimeExtensions.CurrentDateTime, CurrentDevice.ErrorCount + 1);
                return new("上传失败");
            }
        }
        catch (Exception ex)
        {
            CurrentDevice.SetDeviceStatus(DateTimeExtensions.CurrentDateTime, CurrentDevice.ErrorCount + 1);
            return new OperResult(ex);
        }

    }

}
