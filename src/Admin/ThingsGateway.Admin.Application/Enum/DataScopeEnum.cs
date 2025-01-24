//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Admin.Application;

public enum DataScopeEnum
{

    /// <summary>
    /// 仅自己
    /// </summary>
    SCOPE_SELF,

    /// <summary>
    /// 所有
    /// </summary>
    SCOPE_ALL,

    /// <summary>
    /// 仅所属组织
    /// </summary>
    SCOPE_ORG,

    /// <summary>
    /// 所属组织及以下
    /// </summary>
    SCOPE_ORG_CHILD,

    /// <summary>
    /// 自定义
    /// </summary>
    SCOPE_ORG_DEFINE,

}
