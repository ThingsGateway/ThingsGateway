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



using NewLife.Serialization;

using Newtonsoft.Json.Linq;

using System.Data;
using System.Linq;
using System.Reflection;

namespace ThingsGateway.Core
{
    /// <summary>
    /// 对象拓展
    /// </summary>
    [SuppressSniffer]
    public static class ObjectExtension
    {
        public static string FirstCharToLower(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            return input.First().ToString().ToLower() + input.Substring(1);
        }

        public static string FirstCharToUpper(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            return input.First().ToString().ToUpper();
        }

        public static string FormatJson(this string json)
        {
            try
            {
                if (json != null && (json.StartsWith("{") || json.StartsWith("[")))
                {
                    return JToken.Parse(json).ToString();
                }
                else
                {
                    return json;
                }
            }
            catch
            {
                return json;
            }
        }

        /// <summary>
        /// 判断类型是否实现某个泛型
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="generic">泛型类型</param>
        /// <returns>bool</returns>
        public static bool HasImplementedRawGeneric(this Type type, Type generic)
        {
            // 检查接口类型
            var isTheRawGenericType = type.GetInterfaces().Any(IsTheRawGenericType);
            if (isTheRawGenericType) return true;

            // 检查类型
            while (type != null && type != typeof(object))
            {
                isTheRawGenericType = IsTheRawGenericType(type);
                if (isTheRawGenericType) return true;
                type = type.BaseType;
            }

            return false;

            // 判断逻辑
            bool IsTheRawGenericType(Type type) => generic == (type.IsGenericType ? type.GetGenericTypeDefinition() : type);
        }
        public static T ToEnum<T>(this string value, T defaultValue) where T : struct, Enum
        {
            if (!string.IsNullOrEmpty(value) && Enum.TryParse<T>(value, ignoreCase: true, out var result))
            {
                return result;
            }

            return defaultValue;
        }
        public static string LastCharToUpper(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            return input.Last().ToString().ToUpper();
        }

        /// <summary>
        /// 构建菜单树形结构
        /// </summary>
        /// <param name="resourceList">菜单列表</param>
        /// <param name="parentId">父ID</param>
        /// <returns>菜单形结构</returns>
        /// <inheritdoc/>
        public static List<SysResource> ResourceListToTree(this List<SysResource> resourceList, long parentId = 0)
        {
            //找下级资源ID列表
            var resources = resourceList
               .Where(it => it.ParentId == parentId).OrderBy(it => it.SortCode).ToList();
            if (resources.Count > 0)//如果数量大于0
            {
                var data = new List<SysResource>();
                foreach (var item in resources)//遍历资源
                {
                    item.Children = ResourceListToTree(resourceList, item.Id);//添加子节点
                    data.Add(item);//添加到列表
                }
                return data;//返回结果
            }
            return new List<SysResource>();
        }

        /// <summary>
        /// List转DataTable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(this List<T> list)
        {
            DataTable result = new();
            if (list.Count > 0)
            {
                // result.TableName = list[0].GetType().Name; // 表名赋值
                PropertyInfo[] propertys = list[0].GetType().GetProperties();
                foreach (PropertyInfo pi in propertys)
                {
                    Type colType = pi.PropertyType;
                    if (colType.IsGenericType && colType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        colType = colType.GetGenericArguments()[0];
                    }
                    if (IsIgnoreColumn(pi))
                        continue;
                    if (IsJsonColumn(pi))//如果是json特性就是sting类型
                        colType = typeof(string);
                    if (colType.IsEnum)//如果是Enum需要转string才会保存Enum字符串
                        colType = typeof(string);
                    result.Columns.Add(pi.Name, colType);
                }
                for (int i = 0; i < list.Count; i++)
                {
                    ArrayList tempList = new();
                    foreach (PropertyInfo pi in propertys)
                    {
                        if (IsIgnoreColumn(pi))
                            continue;
                        object obj = pi.GetValue(list[i], null);
                        if (IsJsonColumn(pi))//如果是json特性就是转化为json格式
                            obj = obj?.ToJson();//如果json字符串是空就传null
                        tempList.Add(obj);
                    }
                    object[] array = tempList.ToArray();
                    result.LoadDataRow(array, true);
                }
            }
            return result;
        }

        /// <summary>
        /// 多个树转列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="list"></param>
        public static List<T> TreeToList<T>(this IList<T> data) where T : class, ITree<T>
        {
            List<T> list = new List<T>();
            foreach (var item in data)
            {
                list.Add(item);
                if (item.Children != null && item.Children.Count > 0)
                {
                    list.AddRange(item.Children.TreeToList());
                }
            }
            return list;
        }

        /// <summary>
        /// 多个树转列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="list"></param>
        public static List<T> TreeToList<T>(this IList<T> data, long id, bool needParent = false) where T : class, ITree<T>
        {
            List<T> list = new List<T>();
            foreach (var item in data.Where(it => it.ParentId == id))
            {
                list.Add(item);
                if (item.Children != null && item.Children.Count > 0)
                {
                    list.AddRange(item.Children.TreeToList<T>());
                }
            }
            if (needParent)
            {
                list.Add(data.FirstOrDefault(it => it.Id == id));
            }
            return list;
        }

        /// <summary>
        /// 排除SqlSugar忽略的列
        /// </summary>
        /// <param name="pi"></param>
        /// <returns></returns>
        private static bool IsIgnoreColumn(PropertyInfo pi)
        {
            var sc = pi.GetCustomAttributes<SugarColumn>(false).FirstOrDefault(u => u.IsIgnore == true);
            return sc != null;
        }

        private static bool IsJsonColumn(PropertyInfo pi)
        {
            var sc = pi.GetCustomAttributes<SugarColumn>(false).FirstOrDefault(u => u.IsJson == true);
            return sc != null;
        }
    }
}