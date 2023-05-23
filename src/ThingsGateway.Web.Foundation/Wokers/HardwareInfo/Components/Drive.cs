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

namespace ThingsGateway.Web.Foundation
{
    /// <summary>
    /// WMI class: Win32_DiskDrive
    /// </summary>
    public class Drive
    {
        /// <summary>
        /// Short description of the object.
        /// </summary>
        [Description("简述")]
        public string Caption { get; set; } = string.Empty;

        /// <summary>
        /// Description of the object.
        /// </summary>
        [Description("描述")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 修订的磁盘驱动器固件所指定的制造商。
        /// </summary>
        [Description("固件版本")]
        public string FirmwareRevision { get; set; } = string.Empty;

        /// <summary>
        /// 物理驱动器的数量给定的驱动器。
        /// </summary>
        [Description("序号")]
        public UInt32 Index { get; set; }

        /// <summary>
        /// Name of the disk drive manufacturer.
        /// </summary>
        [Description("制造商")]
        public string Manufacturer { get; set; } = string.Empty;

        /// <summary>
        /// 制造商的磁盘驱动器的型号。
        /// </summary>
        [Description("型号")]
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// Label by which the object is known.
        /// </summary>
        [Description("名称")]
        public string Name { get; set; } = string.Empty;

        /// <inheritdoc/>
        public List<Partition> PartitionList { get; set; } = new List<Partition>();
        /// <summary>
        /// 这个物理磁盘驱动器上的分区数量所识别出的操作系统。
        /// </summary>
        [Description("分区数量")]
        public UInt32 Partitions { get; set; }

        /// <summary>
        /// 由制造商分配来识别物理介质。
        /// </summary>
        [Description("序列号")]
        public string SerialNumber { get; set; } = string.Empty;

        /// <summary>
        /// Size of the disk drive.
        /// </summary>
        [Description("总大小")]
        public UInt64 Size { get; set; }

    }

    /// <summary>
    /// WMI class: Win32_DiskPartition
    /// </summary>
    public class Partition
    {
        /// <summary>
        /// 显示计算机是否可以从这个分区引导。
        /// </summary>
        [Description("是否引导分区")]
        public Boolean Bootable { get; set; }

        /// <summary>
        /// 分区是活动分区。
        /// 操作系统使用活动分区时从硬盘启动。
        /// </summary>
        [Description("是否活动分区")]
        public Boolean BootPartition { get; set; }

        /// <summary>
        /// Short description of the object.
        /// </summary>
        [Description("简述")]
        public string Caption { get; set; } = string.Empty;

        /// <summary>
        /// Description of the object.
        /// </summary>
        [Description("描述")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 磁盘包含该分区的索引号。
        /// </summary>
        [Description("磁盘分区索引")]
        public UInt32 DiskIndex { get; set; }

        /// <summary>
        /// 指数的分区。
        /// </summary>
        [Description("分区索引")]
        public UInt32 Index { get; set; }

        /// <summary>
        /// Label by which the object is known.
        /// </summary>
        [Description("名称")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 如果这是真的,这是一个主分区。
        /// </summary>
        [Description("是否主分区")]
        public Boolean PrimaryPartition { get; set; }

        /// <summary>
        /// Total size of the partition.
        /// </summary>
        [Description("总大小")]
        public UInt64 Size { get; set; }

        /// <summary>
        /// (以字节为单位)的分区的起始偏移量。
        /// </summary>
        [Description("分区起始偏移量")]
        public UInt64 StartingOffset { get; set; }
        /// <inheritdoc/>
        public List<Volume> VolumeList { get; set; } = new List<Volume>();
    }

    /// <summary>
    /// WMI 类：Win32 逻辑磁盘
    /// </summary>
    public class Volume
    {
        /// <summary>
        /// 对象的简短描述（单行字符串）。
        /// </summary>
        [Description("简述")]
        public string Caption { get; set; } = string.Empty;

        /// <summary>
        /// Description of the object.
        /// </summary>
        [Description("描述")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 逻辑磁盘上的文件系统。
        /// </summary>
        [Description("文件系统")]
        public string FileSystem { get; set; } = string.Empty;

        /// <summary>
        ///逻辑磁盘上可用的空间（以字节为单位）。
        /// </summary>
        [Description("可用空间")]
        public UInt64 FreeSpace { get; set; }

        /// <summary>
        /// 已知对象的标签。
        /// </summary>
        [Description("名称")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 磁盘驱动器的大小。
        /// </summary>
        [Description("总大小")]
        public UInt64 Size { get; set; }

        /// <summary>
        /// 逻辑磁盘的卷名。
        /// </summary>
        [Description("卷名")]
        public string VolumeName { get; set; } = string.Empty;

        /// <summary>
        /// 逻辑磁盘的卷序列号。
        /// </summary>
        [Description("卷序列号")]
        public string VolumeSerialNumber { get; set; } = string.Empty;

    }
}
