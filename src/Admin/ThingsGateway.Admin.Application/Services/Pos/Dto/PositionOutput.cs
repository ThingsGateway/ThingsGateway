// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

namespace ThingsGateway.Admin.Application;


public class PositionTreeOutput
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 是否是职位
    /// </summary>
    public bool IsPosition { get; set; }

    /// <summary>
    /// 子项
    /// </summary>
    public List<PositionTreeOutput> Children { get; set; } = new List<PositionTreeOutput>();
}


public class PositionSelectorOutput
{
    /// <summary>
    /// 组织Id或者职位Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 子项
    /// </summary>
    public List<PositionSelectorOutput> Children { get; set; } = new List<PositionSelectorOutput>();
}
