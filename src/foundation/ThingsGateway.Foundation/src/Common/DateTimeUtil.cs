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

using NewLife.Threading;

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// DateTimeUtil
    /// </summary>
    public static class DateTimeUtil
    {
        /// <summary>
        /// 系统默认使用的当前时间
        /// </summary>
        public static DateTime Now => DateTime.Now;

        /// <summary>
        /// 500ms变化一次的时间，在频繁获取系统时间时使用
        /// </summary>
        public static DateTime TimerXNow => TimerX.Now;
    }
}