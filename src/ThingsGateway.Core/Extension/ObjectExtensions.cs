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

using NewLife;

using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ThingsGateway.Core
{
    /// <summary>
    /// 扩展方法,部分代码来源为开源代码收集等
    /// </summary>
    public static class ObjectExtensions
    {
        private static BlazorCacheService BlazorCacheService;

        static ObjectExtensions()
        {
            BlazorCacheService = App.GetService<BlazorCacheService>();
        }

        /// <summary>
        /// 获取 DisplayName属性名称
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="accessor"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static string Description<T>(this T item, Expression<Func<T, object>> accessor)
        {
            if (accessor.Body == null)
            {
                throw new ArgumentNullException(nameof(accessor));
            }

            var expression = accessor.Body;
            if (expression is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Convert && unaryExpression.Type == typeof(object))
            {
                expression = unaryExpression.Operand;
            }

            if (expression is not MemberExpression memberExpression)
            {
                throw new ArgumentException("不能访问器(字段、属性)的一个对象。");
            }

            return typeof(T).GetDescription(memberExpression.Member.Name) ?? memberExpression.Member.Name;
        }

        /// <summary>
        /// 获得类型
        /// </summary>
        /// <param name="modelType"></param>
        /// <returns></returns>
        public static IEnumerable<FieldInfo> GetAllFields(this Type modelType)
        {
            var cacheKey = $"{nameof(GetAllFields)}-{CultureInfo.CurrentUICulture.Name}-{modelType.FullName}";
            IEnumerable<FieldInfo> displayName = BlazorCacheService.GetOrAdd("", cacheKey, entry =>
            {
                var fields = modelType.GetRuntimeFields().Where(a => a.IsPublic);
                fields = fields.OrderBy(a =>
                {
                    var order = a.GetCustomAttribute<OrderTableAttribute>()?.Order;
                    if (order != null)
                        return order;
                    else
                        return 999;
                });
                return fields;
            });
            return displayName;
        }

        /// <summary>
        /// 获得类型
        /// </summary>
        /// <param name="modelType"></param>
        /// <returns></returns>
        public static IEnumerable<PropertyInfo> GetAllProps(this Type modelType)
        {
            var cacheKey = $"{nameof(GetAllProps)}-{CultureInfo.CurrentUICulture.Name}-{modelType.FullName}";
            IEnumerable<PropertyInfo> displayName = BlazorCacheService.GetOrAdd("", cacheKey, entry =>
            {
                var props = modelType.GetRuntimeProperties().Where(a => a.GetMethod.IsPublic);
                props = props.OrderBy(a =>
                {
                    var order = a.GetCustomAttribute<OrderTableAttribute>()?.Order;
                    if (order != null)
                        return order;
                    else
                        return 999;
                });
                return props;
            });
            return displayName;
        }
        /// <summary>
        /// 获得类型
        /// </summary>
        /// <param name="modelType"></param>
        /// <returns></returns>
        public static List<string> GetAllPropsName(this Type modelType)
        {
            var displayName = modelType.GetAllProps().Select(it => it.Name);
            return displayName.ToList();
        }

        /// <summary>
        /// 获得类型属性的描述信息
        /// </summary>
        /// <param name="modelType"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static string GetDescription(this Type modelType, string fieldName)
        {
            var cacheKey = $"{nameof(GetDescription)}-{CultureInfo.CurrentUICulture.Name}-{modelType.FullName}-{fieldName}";
            var displayName = BlazorCacheService.GetOrAdd("", cacheKey, entry =>
            {
                string dn = null;
                {
                    var fields = modelType.GetAllFields();
                    var info = fields.FirstOrDefault(p => p.Name == fieldName); ;
                    if (info != null)
                    {
                        dn = FindDisplayAttribute(info);
                    }
                    else
                    {
                        var props = modelType.GetAllProps();

                        var propertyInfo = props.FirstOrDefault(p => p.Name == fieldName);

                        dn = FindDisplayAttribute(propertyInfo);
                    }
                }

                return dn;
            });

            return displayName ?? fieldName;
        }

        public static string FindDisplayAttribute(this MemberInfo memberInfo, Func<MemberInfo, string> func = null)
        {
            var dn = memberInfo.GetCustomAttribute<DisplayAttribute>(true)?.Name
                ?? memberInfo.GetCustomAttribute<DisplayNameAttribute>(true)?.DisplayName
                ?? memberInfo.GetCustomAttribute<DescriptionAttribute>(true)?.Description
                ?? memberInfo.GetCustomAttribute<SugarColumn>(true)?.ColumnDescription
                ?? func?.Invoke(memberInfo);

            return dn;
        }

        /// <summary>
        /// 获取枚举类型的所有项，返回集合
        /// </summary>
        /// <param name="enumType">枚举类型</param>
        public static List<(string name, string des, int value)> GetEnumList(this Type enumType)
        {
            List<(string, string, int)> list = new List<(string, string, int)>();
            var fieldInfos = enumType.GetAllFields().ToList();
            for (int i = 1; i < fieldInfos.Count; i++)
            {
                var item = fieldInfos[i];
                var des = fieldInfos[i].GetCustomAttribute<DisplayNameAttribute>(true)?.DisplayName;
                int num = (int)Enum.Parse(enumType, item.Name);
                if (des.IsNullOrEmpty()) des = item.Name;
                list.Add((des, item.Name, num));
            }
            return list;
        }

        public static string GetNameLen2(this string name)
        {
            var nameLength = name.Length;//获取姓名长度
            string nameWritten = name;//需要绘制的文字
            if (nameLength > 2)//如果名字长度超过2个
            {
                // 如果用户输入的姓名大于等于3个字符，截取后面两位
                string firstName = name.Substring(0, 1);
                if (IsChinese(firstName))
                {
                    // 截取倒数两位汉字
                    nameWritten = name.Substring(name.Length - 2);
                }
                else
                {
                    // 截取第一个英文字母和第二个大写的字母
                    var data = Regex.Match(name, @"[A-Z]?[a-z]+([A-Z])").Value;
                    nameWritten = data.FirstCharToUpper() + data.LastCharToUpper();
                    if (nameWritten.IsNullOrWhiteSpace())
                    {
                        nameWritten = name.FirstCharToUpper() + name.LastCharToUpper();
                    }
                }
            }

            return nameWritten;
        }

        public static List<int> GetOrderNum(int max, int min = 1)
        {
            return Enumerable.Range(min, max).ToList();
        }

        /// <summary>
        /// 获得类型属性的值
        /// </summary>
        /// <param name="modelType"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static string GetPropValue(this Type modelType, object forObject, string fieldName)
        {
            string dn = null;
            var info = modelType.GetAllFields().FirstOrDefault(p => p.Name == fieldName); ;
            if (info != null)
            {
                dn = info.GetValue(forObject)?.ToString();
            }
            else
            {
                var props = modelType.GetAllProps();

                var propertyInfo = props.FirstOrDefault(p => p.Name == fieldName);
                if (propertyInfo != null)
                    if (propertyInfo.PropertyType != typeof(DateTime))
                    {
                        dn = propertyInfo.GetValue(forObject)?.ToString();
                    }
                    else
                    {
                        dn = (propertyInfo.GetValue(forObject)).ToDateTime().ToFullString(true);
                    }
            }

            return dn;
        }



        public static BaseIdInput ToIdInput(this long id)
        {
            return new BaseIdInput() { Id = id };
        }

        /// <summary>
        /// 分页拓展
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static Task<SqlSugarPagedList<TEntity>> ToPagedListAsync<TEntity>(this IEnumerable<TEntity> entity, BasePageInput basePageInput = null, bool isAll = false)
            where TEntity : new()
        {
            if (isAll)
            {
                entity = Sort(basePageInput, entity);
                var data = new SqlSugarPagedList<TEntity>
                {
                    Current = 1,
                    Size = entity?.Count() ?? 0,
                    Records = entity,
                    Total = entity?.Count() ?? 0,
                    Pages = 1,
                    HasNextPages = false,
                    HasPrevPages = false
                };
                return Task.FromResult(data);
            }

            int _PageIndex = basePageInput.Current;
            int _PageSize = basePageInput.Size;
            var num = entity.Count();
            var pageConut = (double)num / _PageSize;
            int PageConut = (int)Math.Ceiling(pageConut);
            IEnumerable<TEntity> list = new List<TEntity>();
            entity = Sort(basePageInput, entity);
            if (PageConut >= _PageIndex)
            {
                list = entity.Skip((_PageIndex - 1) * _PageSize).Take(_PageSize);
            }
            return Task.FromResult(new SqlSugarPagedList<TEntity>
            {
                Current = _PageIndex,
                Size = _PageSize,
                Records = list,
                Total = entity?.Count() ?? 0,
                Pages = PageConut,
                HasNextPages = _PageIndex < PageConut,
                HasPrevPages = _PageIndex - 1 > 0
            });

            static IEnumerable<TEntity> Sort(BasePageInput basePageInput, IEnumerable<TEntity> list)
            {
                if (basePageInput != null && basePageInput.SortField != null)
                {
                    var pro = typeof(TEntity).GetRuntimeProperty(basePageInput.SortField);
                    if (basePageInput.SortOrder == "asc")
                        list = list.OrderBy(a => pro.GetValue(a));
                    else
                        list = list.OrderByDescending(a => pro.GetValue(a));
                }
                return list;
            }
        }

        /// <summary>
        /// 用 正则表达式 判断字符是不是汉字
        /// </summary>
        /// <param name="text">待判断字符或字符串</param>
        /// <returns>真：是汉字；假：不是</returns>
        private static bool IsChinese(string text)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(text, @"[\u4e00-\u9fbb]");
        }
    }
}