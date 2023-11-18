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

//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation.WebApi
{
    /// <summary>
    /// 表示WebApi路由。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
    public class RouterAttribute : Attribute
    {
        /// <summary>
        /// 表示WebApi路由。
        /// 该模板在用于方法时，会覆盖类的使用。
        /// 模板必须由“/”开始，如果没有设置，会自动补齐。
        /// 模板不支持参数约定，仅支持方法路由。
        /// <para>模板有以下约定：
        /// <list type="number">
        /// <item>不区分大小写</item>
        /// <item>以“[Api]”表示当前类名，如果不包含此字段，则意味着会使用绝对设置</item>
        /// <item>以“[Action]”表示当前方法名，如果不包含此字段，则意味着会使用绝对设置</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="routeTemple"></param>
        public RouterAttribute(string routeTemple)
        {
            if (!routeTemple.StartsWith("/"))
            {
                routeTemple = routeTemple.Insert(0, "/");
            }
            this.RouteTemple = routeTemple;
        }

        /// <summary>
        /// 路由模板。
        /// </summary>
        public string RouteTemple { get; }
    }
}