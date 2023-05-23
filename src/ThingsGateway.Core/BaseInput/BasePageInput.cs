﻿#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using Furion.DataValidation;

using System.ComponentModel.DataAnnotations;

namespace ThingsGateway.Core
{
    /// <summary>
    /// 全局分页查询输入参数
    /// </summary>
    public class BasePageInput
    {
        /// <summary>
        /// 当前页码
        /// </summary>
        [DataValidation(ValidationTypes.Numeric)]
        public virtual int Current { get; set; } = 1;

        /// <summary>
        /// 关键字
        /// </summary>
        [Description("关键字")]
        public virtual string SearchKey { get; set; }

        /// <summary>
        /// 每页条数
        /// </summary>
        [Range(1, 100, ErrorMessage = "页码容量超过最大限制")]
        [DataValidation(ValidationTypes.Numeric)]
        public virtual int Size { get; set; } = 10;

        /// <summary>
        /// 排序字段
        /// </summary>
        public virtual string SortField { get; set; }

        /// <summary>
        /// 排序方式，升序：ascend；降序：descend"
        /// </summary>
        public virtual string SortOrder { get; set; } = "desc";
    }
}