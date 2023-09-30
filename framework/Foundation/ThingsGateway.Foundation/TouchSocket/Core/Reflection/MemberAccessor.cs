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

using System.Linq.Expressions;
using System.Reflection;

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// 动态成员访问器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MemberAccessor<T> : MemberAccessor
    {
        /// <summary>
        /// 动态成员访问器
        /// </summary>
        public MemberAccessor() : base(typeof(T))
        {
        }
    }

    /// <summary>
    /// 动态成员访问器
    /// </summary>
    public class MemberAccessor : IMemberAccessor
    {
        private Func<object, string, object> GetValueDelegate;
        private Action<object, string, object> SetValueDelegate;

        /// <summary>
        /// 动态成员访问器
        /// </summary>
        /// <param name="type"></param>
        public MemberAccessor(Type type)
        {
            this.Type = type;
            this.OnGetFieldInfes = (t) => { return t.GetFields(); };
            this.OnGetProperties = (t) => { return t.GetProperties(); };
        }

        private Dictionary<string, FieldInfo> dicFieldInfes;
        private Dictionary<string, PropertyInfo> dicProperties;

        /// <summary>
        /// 构建
        /// </summary>
        public void Build()
        {
            if (GlobalEnvironment.DynamicBuilderType == DynamicBuilderType.Reflect)
            {
                this.dicFieldInfes = this.OnGetFieldInfes.Invoke(this.Type).ToDictionary(a => a.Name);
                this.dicProperties = this.OnGetProperties.Invoke(this.Type).ToDictionary(a => a.Name);
            }

            this.GetValueDelegate = this.GenerateGetValue();
            this.SetValueDelegate = this.GenerateSetValue();
        }

        /// <summary>
        /// 获取属性
        /// </summary>
        public Func<Type, PropertyInfo[]> OnGetProperties { get; set; }

        /// <summary>
        /// 获取字段
        /// </summary>
        public Func<Type, FieldInfo[]> OnGetFieldInfes { get; set; }

        /// <summary>
        /// 所属类型
        /// </summary>
        public Type Type { get; }

        /// <inheritdoc/>
        public object GetValue(object instance, string memberName)
        {
            return this.GetValueDelegate(instance, memberName);
        }

        /// <inheritdoc/>
        public void SetValue(object instance, string memberName, object newValue)
        {
            this.SetValueDelegate(instance, memberName, newValue);
        }

        private Func<object, string, object> GenerateGetValue()
        {
            if (GlobalEnvironment.DynamicBuilderType == DynamicBuilderType.Reflect)
            {
                return (obj, key) =>
                {
                    if (this.dicFieldInfes.TryGetValue(key, out var value1))
                    {
                        return value1.GetValue(obj);
                    }
                    else
                    {
                        return this.dicProperties.TryGetValue(key, out var value2) ? value2.GetValue(obj) : default;
                    }
                };
            }

            var instance = Expression.Parameter(typeof(object), "instance");
            var memberName = Expression.Parameter(typeof(string), "memberName");
            var nameHash = Expression.Variable(typeof(int), "nameHash");
            var calHash = Expression.Assign(nameHash, Expression.Call(memberName, typeof(object).GetMethod("GetHashCode")));
            var cases = new List<SwitchCase>();
            foreach (var propertyInfo in this.OnGetFieldInfes.Invoke(this.Type))
            {
                try
                {
                    var property = Expression.Field(Expression.Convert(instance, this.Type), propertyInfo.Name);
                    var propertyHash = Expression.Constant(propertyInfo.Name.GetHashCode(), typeof(int));

                    cases.Add(Expression.SwitchCase(Expression.Convert(property, typeof(object)), propertyHash));
                }
                catch
                {
                }
            }
            foreach (var propertyInfo in this.OnGetProperties.Invoke(this.Type))
            {
                try
                {
                    var property = Expression.Property(Expression.Convert(instance, this.Type), propertyInfo.Name);
                    var propertyHash = Expression.Constant(propertyInfo.Name.GetHashCode(), typeof(int));

                    cases.Add(Expression.SwitchCase(Expression.Convert(property, typeof(object)), propertyHash));
                }
                catch
                {
                }
            }
            if (cases.Count == 0)
            {
                return (a, b) => default;
            }
            var switchEx = Expression.Switch(nameHash, Expression.Constant(null), cases.ToArray());
            var methodBody = Expression.Block(typeof(object), new[] { nameHash }, calHash, switchEx);

            return Expression.Lambda<Func<object, string, object>>(methodBody, instance, memberName).Compile();
        }

        private Action<object, string, object> GenerateSetValue()
        {
            if (GlobalEnvironment.DynamicBuilderType == DynamicBuilderType.Reflect)
            {
                return (obj, key, value) =>
                {
                    if (this.dicFieldInfes.TryGetValue(key, out var value1))
                    {
                        value1.SetValue(obj, value);
                    }
                    if (this.dicProperties.TryGetValue(key, out var value2))
                    {
                        value2.SetValue(obj, value);
                    }
                };
            }

            var instance = Expression.Parameter(typeof(object), "instance");
            var memberName = Expression.Parameter(typeof(string), "memberName");
            var newValue = Expression.Parameter(typeof(object), "newValue");
            var nameHash = Expression.Variable(typeof(int), "nameHash");
            var calHash = Expression.Assign(nameHash, Expression.Call(memberName, typeof(object).GetMethod("GetHashCode")));
            var cases = new List<SwitchCase>();
            foreach (var propertyInfo in this.OnGetFieldInfes.Invoke(this.Type))
            {
                var property = Expression.Field(Expression.Convert(instance, this.Type), propertyInfo.Name);
                var setValue = Expression.Assign(property, Expression.Convert(newValue, propertyInfo.FieldType));
                var propertyHash = Expression.Constant(propertyInfo.Name.GetHashCode(), typeof(int));

                cases.Add(Expression.SwitchCase(Expression.Convert(setValue, typeof(object)), propertyHash));
            }
            foreach (var propertyInfo in this.OnGetProperties(this.Type))
            {
                if (!propertyInfo.CanWrite)
                {
                    continue;
                }
                var property = Expression.Property(Expression.Convert(instance, this.Type), propertyInfo.Name);
                var setValue = Expression.Assign(property, Expression.Convert(newValue, propertyInfo.PropertyType));
                var propertyHash = Expression.Constant(propertyInfo.Name.GetHashCode(), typeof(int));

                cases.Add(Expression.SwitchCase(Expression.Convert(setValue, typeof(object)), propertyHash));
            }
            if (cases.Count == 0)
            {
                return (a, b, c) => { };
            }
            var switchEx = Expression.Switch(nameHash, Expression.Constant(null), cases.ToArray());
            var methodBody = Expression.Block(typeof(object), new[] { nameHash }, calHash, switchEx);

            return Expression.Lambda<Action<object, string, object>>(methodBody, instance, memberName, newValue).Compile();
        }
    }
}