//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using NewLife.Data;

namespace NewLife.Algorithms;

/// <summary>
/// 插值算法
/// </summary>
public interface IInterpolation
{
    /// <summary>
    /// 插值处理
    /// </summary>
    /// <param name="data">数据</param>
    /// <param name="prev">上一个点索引</param>
    /// <param name="next">下一个点索引</param>
    /// <param name="current">当前点时间值</param>
    /// <returns></returns>
    Double Process(TimePoint[] data, Int32 prev, Int32 next, Int64 current);
}