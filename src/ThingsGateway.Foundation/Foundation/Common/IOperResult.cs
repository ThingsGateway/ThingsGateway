//返回结果标准化，参照RESULTAPI
namespace ThingsGateway.Foundation
{
    public interface IOperResult
    {
        bool IsSuccess { get; }
        string Message { get; set; }
        ResultCode ResultCode { get; set; }
    }
}