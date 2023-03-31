//返回结果标准化，参照RESULTAPI
namespace ThingsGateway.Foundation
{
    /// <summary>
    /// 操作接口
    /// </summary>
    public interface IOperResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        bool IsSuccess { get; }
        /// <summary>
        /// 返回消息
        /// </summary>
        string Message { get; set; }
        /// <summary>
        /// 操作代码
        /// </summary>
        ResultCode ResultCode { get; set; }
    }
}