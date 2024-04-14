//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Gateway.Application
{
    public interface IDBHistoryAlarm
    {
        public string Name { get; set; }

        public string? Description { get; set; }

        public string DeviceName { get; set; }

        public string RegisterAddress { get; set; }

        public DataTypeEnum DataType { get; set; }

        public string AlarmCode { get; set; }

        public string AlarmLimit { get; set; }

        public string? AlarmText { get; set; }

        public DateTime AlarmTime { get; set; }

        public DateTime EventTime { get; set; }

        public AlarmTypeEnum? AlarmType { get; set; }

        public EventTypeEnum EventType { get; set; }

        public string Remark1 { get; set; }

        public string Remark2 { get; set; }

        public string Remark3 { get; set; }

        public string Remark4 { get; set; }
    }
}