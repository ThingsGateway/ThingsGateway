﻿namespace ThingsGateway.Foundation.Extension
{
    /// <inheritdoc/>
    public static class OperResultExtensions
    {
        #region Public Methods

        /// <summary>
        /// 复制信息，不包含泛型类
        /// </summary>
        public static OperResult<T> Copy<T>(this OperResult result)
        {
            OperResult<T> failedResult = new OperResult<T>(result.ResultCode, result.Message)
            {
            };
            return failedResult;
        }

        /// <summary>
        /// 复制信息，不包含泛型类
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="result"></param>
        /// <returns></returns>
        public static OperResult<T1, T2> Copy<T1, T2>(this OperResult result)
        {
            OperResult<T1, T2> failedResult = new OperResult<T1, T2>(result.ResultCode, result.Message)
            {
            };
            return failedResult;
        }
        /// <summary>
        /// 复制信息
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static OperResult Copy(this OperResult result)
        {
            OperResult failedResult = new OperResult(result.ResultCode, result.Message)
            {
            };
            return failedResult;
        }
        /// <summary>
        /// 操作成功则继续
        /// </summary>
        /// <param name="result"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static OperResult Then(this OperResult result, Func<OperResult> func)
        {
            return !result.IsSuccess ? result : func();
        }
        /// <inheritdoc cref="Then(OperResult, Func{OperResult})"/>
        public static OperResult<T> Then<T>(this OperResult result, Func<OperResult<T>> func)
        {
            return !result.IsSuccess ? result.Copy<T>() : func();
        }

        /// <inheritdoc cref="Then(OperResult, Func{OperResult})"/>
        public static OperResult<T1, T2> Then<T1, T2>(this OperResult result, Func<OperResult<T1, T2>> func)
        {
            return !result.IsSuccess ? result.Copy<T1, T2>() : func();
        }

        #endregion Public Methods
    }
}