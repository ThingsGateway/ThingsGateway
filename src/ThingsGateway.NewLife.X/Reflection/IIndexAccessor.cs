
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------




namespace NewLife.Reflection
{
    /// <summary>
    /// 索引器接访问口。
    /// 该接口用于通过名称快速访问对象属性或字段（属性优先）。
    /// </summary>
    //[Obsolete("=>IIndex")]
    public interface IIndexAccessor
    {
        /// <summary>获取/设置 指定名称的属性或字段的值</summary>
        /// <param name="name">名称</param>
        /// <returns></returns>
        Object this[String name] { get; set; }
    }
}