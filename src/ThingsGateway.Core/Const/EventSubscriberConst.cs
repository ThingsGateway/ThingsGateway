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

namespace ThingsGateway.Core
{
    /// <summary>
    /// 事件总线常量
    /// </summary>
    public class EventSubscriberConst
    {
        /// <summary>
        /// 清除用户缓存
        /// </summary>
        public const string ClearUserCache = "清除用户缓存";

        /// <summary>
        /// 页面登录
        /// </summary>
        public const string Login = "登录";

        /// <summary>
        /// OpenApi登录
        /// </summary>
        public const string LoginOpenApi = "OpenApi登录";

        /// <summary>
        /// 页面登录
        /// </summary>
        public const string LoginOut = "退出";

        /// <summary>
        /// 页面登录
        /// </summary>
        public const string LoginOutOpenApi = "OpenApi退出";

        /// <summary>
        /// 操作日志
        /// </summary>
        public const string LogOperate = "操作日志";

        /// <summary>
        /// 访问日志
        /// </summary>
        public const string LogVisit = "访问日志";
    }
}