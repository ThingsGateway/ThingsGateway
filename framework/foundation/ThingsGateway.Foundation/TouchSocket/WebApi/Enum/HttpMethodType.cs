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
    /// 请求函数类型
    /// </summary>
    public enum HttpMethodType
    {
        /// <summary>
        /// 以GET方式。支持调用上下文。
        /// <para>以该方式时，所有的参数类型必须是基础类型。所有的参数来源均来自url参数。</para>
        /// </summary>
        GET,

        /// <summary>
        /// 以Post方式。支持调用上下文。
        /// <para>以该方式时，可以应对以下情况：</para>
        /// <list type="bullet">
        /// <item>仅有一个参数时，该参数可以为任意类型，且参数来源为Body</item>
        /// <item>当有多个参数时，最后一个参数可以为任意类型，且参数来源为Body，其余参数均必须是基础类型，且来自url参数。</item>
        /// </list>
        /// </summary>
        POST
    }
}