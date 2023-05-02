using Microsoft.Extensions.Logging;

using System.Threading;

namespace ThingsGateway.Web.Foundation;


/// <summary>
/// 上传插件
/// <para></para>
/// 约定：
/// <para></para>
/// 如果设备属性需要密码输入，属性名称中需包含Password字符串
/// <para></para>
/// 如果设备属性需要大文本输入，属性名称中需包含BigText字符串
/// </summary>
public abstract class UpLoadBase : DriverBase
{

    /// <inheritdoc cref="UpLoadBase"/>
    public UpLoadBase(IServiceScopeFactory scopeFactory) : base(scopeFactory)
    {
    }

    /// <summary>
    /// 返回插件的上传变量，一般在<see cref="Init(UploadDevice)"/>后初始化
    /// </summary>
    public abstract List<CollectVariableRunTime> UploadVariables { get; }

    /// <summary>
    /// 插件配置项 ，继承实现<see cref="VariablePropertyBase"/>后，返回继承类，如果不存在，返回null
    /// </summary>
    public abstract VariablePropertyBase VariablePropertys { get; }



    /// <summary>
    /// 开始执行的方法
    /// </summary>
    /// <returns></returns>
    public abstract Task BeforStartAsync();

    /// <summary>
    /// 循环执行
    /// </summary>
    public abstract Task ExecuteAsync(CancellationToken cancellationToken);

    /// <summary>
    /// 初始化
    /// </summary>
    public void Init(ILogger logger, UploadDevice device)
    {
        _logger = logger;
        IsLogOut = device.IsLogOut;
        Init(device);
    }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="device">设备</param>
    protected abstract void Init(UploadDevice device);


}
