using Furion.FriendlyException;

namespace ThingsGateway.Core
{
    /// <summary>
    /// 通用错误码
    /// </summary>
    [ErrorCodeType]
    public enum ErrorCodeEnum
    {
        /// <summary>
        /// 系统异常
        /// </summary>
        [ErrorCodeItemMetadata("系统异常")]
        A0000,

        /// <summary>
        /// 数据不存在
        /// </summary>
        [ErrorCodeItemMetadata("数据不存在")]
        A0001,

        /// <summary>
        /// 删除失败
        /// </summary>
        [ErrorCodeItemMetadata("删除失败")]
        A0002,

        /// <summary>
        /// 操作失败
        /// </summary>
        [ErrorCodeItemMetadata("操作失败")]
        A0003,

        /// <summary>
        /// 没有权限
        /// </summary>
        [ErrorCodeItemMetadata("没有权限")]
        A0004,
    }
}