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

namespace ThingsGateway.Gateway.Core;

/// <summary>
/// 连读报文信息
/// </summary>
public class DeviceVariableSourceRead : IDeviceVariableSourceRead<IDeviceVariableRunTime>
{
    /// <summary>
    /// 读取地址，传入时需要去除额外信息
    /// </summary>
    public string Address { get; set; }

    /// <summary>
    /// 需分配的变量列表
    /// </summary>
    public List<IDeviceVariableRunTime> DeviceVariableRunTimes { get; set; } = new();

    /// <summary>
    /// 间隔时间实现
    /// </summary>
    public TimerTick IntervalTimeTick { get; set; }
    /// <summary>
    /// 读取长度
    /// </summary>
    public int Length { get; set; }

    /// <summary>
    /// 检测是否达到读取间隔
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public bool CheckIfRequestAndUpdateTime(DateTime time) => IntervalTimeTick.IsTickHappen(time);

    /// <summary>
    /// 离线原因
    /// </summary>
    public string LastErrorMessage => DeviceVariableRunTimes?.FirstOrDefault()?.LastErrorMessage ?? string.Empty;

    public void SetValue(string lastErrorMessage = null)
    {
        foreach (var item in DeviceVariableRunTimes)
        {
            item.SetValue(null, lastErrorMessage: lastErrorMessage);
        }
    }
}
