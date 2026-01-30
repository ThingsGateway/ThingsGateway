//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 业务插件
/// </summary>
public abstract class BusinessBaseWithCacheIntervalVariable : BusinessBaseWithCacheInterval
{
    protected override bool PluginEventDataModelEnable { get; set; } = false;
    protected override bool AlarmModelEnable { get; set; } = false;

    protected override bool DevModelEnable { get; set; } = false;

    protected override bool VarModelEnable { get; set; } = true;

    protected override ValueTask<OperResult> UpdateDevModel(List<CacheDBItem<DeviceBasicData>> item, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
    protected override ValueTask<OperResult> UpdateDevModels(List<DeviceBasicData> item, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
    protected override ValueTask<OperResult> UpdateAlarmModel(List<CacheDBItem<AlarmVariable>> item, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
    protected override ValueTask<OperResult> UpdatePluginEventDataModel(List<CacheDBItem<PluginEventData>> item, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }


}
