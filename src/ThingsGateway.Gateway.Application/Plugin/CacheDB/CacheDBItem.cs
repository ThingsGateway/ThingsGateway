//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using SqlSugar;

using Yitter.IdGenerator;

namespace ThingsGateway.Gateway.Application;

[SugarTable("CacheDBItem")]
public class CacheDBItem<T> : IPrimaryIdEntity
{
    public CacheDBItem()
    {
        Id = YitIdHelper.NextId();
    }

    public CacheDBItem(T value)
    {
        Id = YitIdHelper.NextId();
        Value = value;
    }

    [SugarColumn(IsPrimaryKey = true)]
    public long Id { get; set; }

    [SugarColumn(IsJson = true)]
    public T Value { get; set; }
}
