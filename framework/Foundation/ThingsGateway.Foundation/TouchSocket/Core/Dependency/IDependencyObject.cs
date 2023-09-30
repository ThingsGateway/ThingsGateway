#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// 依赖对象接口
    /// </summary>
    public interface IDependencyObject : IDisposable
    {
        /// <summary>
        /// 获取依赖注入的值，当没有注入时，会返回默认值。
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dp"></param>
        /// <returns></returns>
        TValue GetValue<TValue>(IDependencyProperty<TValue> dp);

        /// <summary>
        /// 是否有值。
        /// </summary>
        /// <param name="dp"></param>
        /// <returns></returns>
        bool HasValue<TValue>(IDependencyProperty<TValue> dp);

        /// <summary>
        /// 重置属性值。
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dp"></param>
        /// <returns></returns>
        DependencyObject RemoveValue<TValue>(IDependencyProperty<TValue> dp);

        /// <summary>
        /// 设置依赖注入的值
        /// </summary>
        /// <param name="dp"></param>
        /// <param name="value"></param>
        DependencyObject SetValue<TValue>(IDependencyProperty<TValue> dp, TValue value);

        /// <summary>
        /// 尝试获取依赖注入的值，当没有注入时，会返回<see langword="false"/>。
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dp"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool TryGetValue<TValue>(IDependencyProperty<TValue> dp, out TValue value);

        /// <summary>
        /// 重置属性值。
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dp"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool TryRemoveValue<TValue>(IDependencyProperty<TValue> dp, out TValue value);
    }
}