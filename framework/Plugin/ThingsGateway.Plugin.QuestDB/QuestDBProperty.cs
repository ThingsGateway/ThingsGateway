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

namespace ThingsGateway.Plugin.QuestDB;

public class QuestDBProperty : UpDriverPropertyBase
{
    [DeviceProperty("链接字符串", "")] public string ConnectStr { get; set; } = "host=localhost;port=8812;username=admin;password=quest;database=qdb;ServerCompatibilityMode=NoTypeLoading;";
    [DeviceProperty("是否间隔插入", "False时将每次变化写入")] public bool IsInterval { get; set; } = true;
    [DeviceProperty("间隔时间", "秒，实时表时代表更新间隔，历史表时代表插入间隔")] public int IntervalTime { get; set; } = 10;
    [DeviceProperty("缓存最大条数", "默认2千条")] public int CacheMaxCount { get; set; } = 2000;

    /// <summary>
    /// 线程循环间隔
    /// </summary>
    [DeviceProperty("线程循环间隔", "最小10ms")]
    public int CycleInterval { get; set; } = 1000;

}



