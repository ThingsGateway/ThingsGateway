#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/dotnetchina/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

namespace ThingsGateway.Application
{
    /// <summary>
    /// Api授权资源树
    /// </summary>
    public class OpenApiPermissionTreeSelector : ITree<OpenApiPermissionTreeSelector>
    {
        /// <summary>
        /// 接口描述
        /// </summary>
        [Description("Api说明")]
        public string ApiName { get; set; }

        /// <summary>
        /// 路由名称
        /// </summary>
        [Description("Api路径")]
        public string ApiRoute { get; set; }
        /// <summary>
        /// 子节点
        /// </summary>
        public List<OpenApiPermissionTreeSelector> Children { get; set; } = new();
        /// <summary>
        /// ID
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 父ID
        /// </summary>
        public long ParentId { get; set; }

        /// <summary>
        /// 权限名称
        /// </summary>
        [Description("权限名称")]
        public string PermissionName { get; set; }
    }

    /// <summary>
    /// 添加用户参数
    /// </summary>
    public class OpenApiUserAddInput : OpenApiUser
    {
        /// <summary>
        /// 账号
        /// </summary>
        [Required(ErrorMessage = "账号不能为空"), MinLength(3, ErrorMessage = "账号不能少于4个字符")]
        public override string Account { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        [Required(ErrorMessage = "密码不能为空"), MinLength(2, ErrorMessage = "密码不能少于3个字符")]
        public override string Password { get; set; }
    }

    /// <summary>
    /// 编辑用户参数
    /// </summary>
    public class OpenApiUserEditInput : OpenApiUser
    {
        /// <summary>
        /// 账号
        /// </summary>
        [Required(ErrorMessage = "账号不能为空"), MinLength(3, ErrorMessage = "账号不能少于4个字符")]
        public override string Account { get; set; }

        /// <summary>
        /// Id
        /// </summary>
        [MinValue(1, ErrorMessage = "Id不能为空")]
        public override long Id { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        [Required(ErrorMessage = "密码不能为空"), MinLength(2, ErrorMessage = "密码不能少于3个字符")]
        public override string Password { get; set; }
    }

    /// <summary>
    /// 用户分页查询参数
    /// </summary>
    public class OpenApiUserPageInput : BasePageInput
    {
        /// <summary>
        /// 动态查询条件
        /// </summary>
        public Expressionable<SysUser> Expression { get; set; }
    }

    /// <summary>
    /// 用户授权参数
    /// </summary>
    public class OpenApiUserGrantPermissionInput
    {
        /// <summary>
        /// Id
        /// </summary>
        [Required(ErrorMessage = "Id不能为空")]
        public long? Id { get; set; }

        /// <summary>
        /// 授权权限信息
        /// </summary>
        [Required(ErrorMessage = "PermissionList不能为空")]
        public List<string> PermissionList { get; set; }
    }
}