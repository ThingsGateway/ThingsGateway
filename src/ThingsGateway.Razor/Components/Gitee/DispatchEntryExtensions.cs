//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Core;

namespace ThingsGateway.Razor;

/// <summary>
/// DispatchEntry 扩展方法
/// </summary>
public static class DispatchEntryExtensions
{
    /// <summary>
    /// Gitee推送是否应该触发消息弹出
    /// </summary>
    /// <param name="entry"></param>
    /// <returns></returns>
    public static bool CanDispatch(this DispatchEntry<GiteePostBody> entry)
    {
        return entry.Entry != null && (entry.Entry.HeadCommit != null || entry.Entry.Commits?.Count > 0);
    }
}