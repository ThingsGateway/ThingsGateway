namespace ThingsGateway.Foundation
{
    /// <summary>
    /// 所有数据转换类的静态辅助方法<br />
    /// </summary>
    public static class ByteTransformHelper
    {
        public static OperResult<TResult> GetResultFromArray<TResult>(
          OperResult<TResult[]> result)
        {
            return GetSuccessResultFromOther(result, m => m[0]);
        }

        public static OperResult<TResult> GetResultFromBytes<TResult>(
          OperResult<byte[]> result,
          Func<byte[], TResult> translator)
        {
            try
            {
                return result.IsSuccess ? OperResult.CreateSuccessResult(translator(result.Content)) : result.Copy<TResult>();
            }
            catch (Exception ex)
            {
                OperResult<TResult> resultFromBytes = new OperResult<TResult>
                    (
                    string.Format("{0} {1} : Length({2}) {3}", Resource.DataTransError, result.Content.ToHexString(), result.Content.Length, ex.Message)
                    );
                return resultFromBytes;
            }
        }

        public static OperResult GetResultFromOther<TIn>(
                  OperResult<TIn> result,
          Func<TIn, OperResult> trans)
        {
            return !result.IsSuccess ? result : trans(result.Content);
        }

        public static OperResult<TResult> GetResultFromOther<TResult, TIn>(
          OperResult<TIn> result,
          Func<TIn, OperResult<TResult>> trans)
        {
            return !result.IsSuccess ? result.Copy<TResult>() : trans(result.Content);
        }

        public static OperResult<TResult> GetSuccessResultFromOther<TResult, TIn>(
                  OperResult<TIn> result,
          Func<TIn, TResult> trans)
        {
            return !result.IsSuccess ? result.Copy<TResult>() : OperResult.CreateSuccessResult(trans(result.Content));
        }
    }
}