#region copyright
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

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// 所有数据转换类的静态辅助方法<br />
    /// </summary>
    public static class ByteTransformHelper
    {
        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public static OperResult GetResultFromOther<TIn>(
                  OperResult<TIn> result,
          Func<TIn, OperResult> trans)
        {
            return !result.IsSuccess ? result : trans(result.Content);
        }

        /// <inheritdoc/>
        public static OperResult<TResult> GetResultFromOther<TResult, TIn>(
          OperResult<TIn> result,
          Func<TIn, OperResult<TResult>> trans)
        {
            return !result.IsSuccess ? result.Copy<TResult>() : trans(result.Content);
        }

        /// <inheritdoc/>
        public static OperResult<TResult> GetSuccessResultFromOther<TResult, TIn>(
                  OperResult<TIn> result,
          Func<TIn, TResult> trans)
        {
            return !result.IsSuccess ? result.Copy<TResult>() : OperResult.CreateSuccessResult(trans(result.Content));
        }
    }
}