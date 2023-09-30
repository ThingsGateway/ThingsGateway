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

//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// 依赖项对象.
    /// 非线程安全。
    /// </summary>
    public class DependencyObject : DisposableObject, IDependencyObject
    {
        private readonly Dictionary<int, object> m_dp = new Dictionary<int, object>();

        /// <inheritdoc/>
        public TValue GetValue<TValue>(IDependencyProperty<TValue> dp)
        {
            return this.m_dp.TryGetValue(dp.Id, out var value) ? (TValue)value : dp.DefauleValue;
        }

        /// <inheritdoc/>
        public bool HasValue<TValue>(IDependencyProperty<TValue> dp)
        {
            return this.m_dp.ContainsKey(dp.Id);
        }

        /// <inheritdoc/>
        public DependencyObject RemoveValue<TValue>(IDependencyProperty<TValue> dp)
        {
            this.m_dp.Remove(dp.Id);
            return this;
        }

        /// <inheritdoc/>
        public DependencyObject SetValue<TValue>(IDependencyProperty<TValue> dp, TValue value)
        {
            this.m_dp.AddOrUpdate(dp.Id, value);
            return this;
        }

        /// <inheritdoc/>
        public bool TryGetValue<TValue>(IDependencyProperty<TValue> dp, out TValue value)
        {
            if (this.m_dp.TryGetValue(dp.Id, out var value1))
            {
                value = (TValue)value1;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        /// <inheritdoc/>
        public bool TryRemoveValue<TValue>(IDependencyProperty<TValue> dp, out TValue value)
        {
            if (this.m_dp.TryGetValue(dp.Id, out var obj))
            {
                value = (TValue)obj;
                this.m_dp.Remove(dp.Id);
                return true;
            }
            value = default;
            return false;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            this.m_dp.Clear();
            base.Dispose(disposing);
        }
    }
}