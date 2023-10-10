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

using System.Collections.Concurrent;
using System.Reflection;

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    /// DynamicMethodMemberAccessor
    /// </summary>
    public class DynamicMethodMemberAccessor : IMemberAccessor
    {
        private readonly ConcurrentDictionary<Type, IMemberAccessor> m_classAccessors = new ConcurrentDictionary<Type, IMemberAccessor>();

        static DynamicMethodMemberAccessor()
        {
            Default = new DynamicMethodMemberAccessor();
        }

        /// <summary>
        /// DynamicMethodMemberAccessor的默认实例。
        /// </summary>
        public static DynamicMethodMemberAccessor Default { get; private set; }

        /// <summary>
        /// 获取字段
        /// </summary>
        public Func<Type, FieldInfo[]> OnGetFieldInfes { get; set; }

        /// <summary>
        /// 获取属性
        /// </summary>
        public Func<Type, PropertyInfo[]> OnGetProperties { get; set; }

        /// <inheritdoc/>
        public object GetValue(object instance, string memberName)
        {
            return this.FindClassAccessor(instance).GetValue(instance, memberName);
        }

        /// <inheritdoc/>
        public void SetValue(object instance, string memberName, object newValue)
        {
            this.FindClassAccessor(instance).SetValue(instance, memberName, newValue);
        }

        private IMemberAccessor FindClassAccessor(object instance)
        {
            var typekey = instance.GetType();
            if (!this.m_classAccessors.TryGetValue(typekey, out var classAccessor))
            {
                var memberAccessor = new MemberAccessor(instance.GetType());
                if (this.OnGetFieldInfes != null)
                {
                    memberAccessor.OnGetFieldInfes = this.OnGetFieldInfes;
                }

                if (this.OnGetProperties != null)
                {
                    memberAccessor.OnGetProperties = this.OnGetProperties;
                }
                memberAccessor.Build();
                classAccessor = memberAccessor;
                this.m_classAccessors.TryAdd(typekey, classAccessor);
            }
            return classAccessor;
        }
    }
}