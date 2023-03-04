//返回结果标准化，参照RESULTAPI
using TouchSocket.Resources;

namespace ThingsGateway.Foundation
{
    public class OperResult<T> : OperResult
    {
        public OperResult() { }
        public OperResult(ResultCode resultCode, string msg) : base(resultCode, msg)
        {
        }

        public OperResult(T content) : base(ResultCode.Fail)
        {
            Content = content;
        }

        public OperResult(string msg) : base(msg)
        {
        }

        public OperResult(Exception ex) : base(ex)
        {
        }

        public T Content { get; set; }

        public OperResult<TResult> Then<TResult>(Func<T, OperResult<TResult>> func)
        {
            return !IsSuccess ? OperResult.CreateFailedResult<TResult>(this) : func(Content);
        }
    }

    public class OperResult<T1, T2> : OperResult
    {
        public OperResult() : base()
        {
        }
        public OperResult(ResultCode resultCode, string msg) : base(resultCode, msg)
        {
        }

        public OperResult(T1 content1, T2 content2) : base(ResultCode.Fail)
        {
            Content1 = content1; Content2 = content2;
        }

        public OperResult(string msg) : base(msg)
        {
        }

        public OperResult(Exception ex) : base(ex)
        {
        }

        public T1 Content1 { get; set; }
        public T2 Content2 { get; set; }
    }

    public class OperResult<T1, T2, T3> : OperResult
    {
        public OperResult(ResultCode resultCode, string msg) : base(resultCode, msg)
        {
        }

        public OperResult(T1 content1, T2 content2, T3 content3) : base(ResultCode.Fail)
        {
            Content1 = content1; Content2 = content2; Content3 = content3;
        }

        public OperResult(string msg) : base(msg)
        {
        }

        public OperResult(Exception ex) : base(ex)
        {
        }

        public T1 Content1 { get; set; }
        public T2 Content2 { get; set; }
        public T3 Content3 { get; set; }
    }

    public class OperResult : IRequestInfo, IOperResult
    {
        /// <summary>
        /// 业务错误代码
        /// </summary>
        public int Code;

        public OperResult()
        {
        }

        public OperResult(ResultCode resultCode, string msg)
        {
            ResultCode = resultCode;
            Message = msg;
        }

        public OperResult(ResultCode resultCode)
        {
            ResultCode = resultCode;
            Message = ResultCode.GetDescription();
        }

        public OperResult(int code)
        {
            ResultCode = ResultCode.Fail;
            Code = code;
        }

        public OperResult(string msg)
        {
            ResultCode = ResultCode.Fail;
            Message = msg;
        }
        public string Exception { get; set; }
        public OperResult(Exception ex)
        {
            Message = ex.Message;
            Exception = ex.StackTrace;
        }

        public bool IsSuccess => ResultCode.HasFlag(ResultCode.Success);
        public string Message { get; set; }
        public ResultCode ResultCode { get; set; }

        public static OperResult<T1> CreateFailedResult<T1>(OperResult result)
        {
            OperResult<T1> failedResult = new OperResult<T1>(result.Message)
            {
                Code = result.Code
            };
            return failedResult;
        }

        public static OperResult<T1, T2> CreateFailedResult<T1, T2>(OperResult result)
        {
            OperResult<T1, T2> failedResult = new OperResult<T1, T2>(result.Message)
            {
                Code = result.Code
            };
            return failedResult;
        }

        public static OperResult<T1, T2, T3> CreateFailedResult<T1, T2, T3>(OperResult result)
        {
            OperResult<T1, T2, T3> failedResult = new OperResult<T1, T2, T3>(result.Message)
            {
                Code = result.Code
            };
            return failedResult;
        }

        public static OperResult CreateSuccessResult()
        {
            return new OperResult(ResultCode.Success);
        }

        public static OperResult<T> CreateSuccessResult<T>(T value)
        {
            return new OperResult<T>(value)
            {
                ResultCode = ResultCode.Success,
                Message = TouchSocketStatus.Success.GetDescription(),
            };
        }

        public static OperResult<T1, T2> CreateSuccessResult<T1, T2>(T1 value1, T2 value2)
        {
            return new OperResult<T1, T2>(value1, value2)
            {
                ResultCode = ResultCode.Success,
                Message = TouchSocketStatus.Success.GetDescription(),
            };
        }

        public static OperResult<T1, T2, T3> CreateSuccessResult<T1, T2, T3>(T1 value1, T2 value2, T3 value3)
        {
            return new OperResult<T1, T2, T3>(value1, value2, value3)
            {
                ResultCode = ResultCode.Success,
                Message = TouchSocketStatus.Success.GetDescription(),
            };
        }
    }
}