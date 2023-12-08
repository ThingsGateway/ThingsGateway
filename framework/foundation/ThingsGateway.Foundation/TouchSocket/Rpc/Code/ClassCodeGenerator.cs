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
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;

namespace ThingsGateway.Foundation.Rpc
{
    /// <summary>
    /// 代码辅助类
    /// </summary>
    public class ClassCodeGenerator
    {
        private static readonly string[] m_dicType = { "Dictionary`2", "IDictionary`2" };
        private static readonly string[] m_listType = { "List`1", "HashSet`1", "IList`1", "ISet`1", "ICollection`1", "IEnumerable`1" };

        private List<string> tupleElementNames;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="assembly"></param>
        public ClassCodeGenerator(Assembly[] assembly)
        {
            this.Assembly = assembly;
            this.PropertyDic = new ConcurrentDictionary<Type, ClassCellCode>();
            //GenericTypeDic = new ConcurrentDictionary<Type, string>();
        }

        /// <summary>
        /// 程序集
        /// </summary>
        public Assembly[] Assembly { get; private set; }

        ///// <summary>
        ///// 泛型类型字典
        ///// </summary>
        //public ConcurrentDictionary<Type, string> GenericTypeDic { get; private set; }

        /// <summary>
        /// 属性类型字典。
        /// </summary>
        public ConcurrentDictionary<Type, ClassCellCode> PropertyDic { get; private set; }

        /// <summary>
        /// 添加类型字符串
        /// </summary>
        /// <param name="type"></param>
        /// <param name="deep"></param>
        public void AddTypeString(Type type, ref int deep)
        {
            if (CodeGenerator.m_ignoreTypes.Contains(type))
            {
                return;
            }
            if (CodeGenerator.m_ignoreAssemblies.Contains(type.Assembly))
            {
                return;
            }
            deep++;
            if (deep > 50)
            {
                return;
            }
            if (type.IsByRef)
            {
                type = type.GetRefOutType();
            }

            if (type.IsPrimitive || type == typeof(string))
            {
                return;
            }

            if (type == TouchSocketCoreUtility.objType)
            {
                return;
            }
            if (type.IsInterface || type.IsAbstract)
            {
                return;
            }
            if (type.IsArray)
            {
                this.AddTypeString(type.GetElementType(), ref deep);
            }
            else if (type.IsGenericType)
            {
                var types = type.GetGenericArguments();
                foreach (var itemType in types)
                {
                    this.AddTypeString(itemType, ref deep);
                }
            }
            else if (type.IsEnum)
            {
                var baseType = Enum.GetUnderlyingType(type);
                var stringBuilder = new StringBuilder();
                if (baseType == TouchSocketCoreUtility.byteType)
                {
                    stringBuilder.AppendLine($"public enum {type.Name}:byte");
                    stringBuilder.AppendLine("{");
                    var array = Enum.GetValues(type);
                    foreach (var item in array)
                    {
                        var enumString = item.ToString();
                        stringBuilder.AppendLine($"{enumString}={(byte)item},");
                    }
                }
                else if (baseType == TouchSocketCoreUtility.shortType)
                {
                    stringBuilder.AppendLine($"public enum {type.Name}:short");
                    stringBuilder.AppendLine("{");
                    var array = Enum.GetValues(type);
                    foreach (var item in array)
                    {
                        var enumString = item.ToString();
                        stringBuilder.AppendLine($"{enumString}={(short)item},");
                    }
                }
                else if (baseType == TouchSocketCoreUtility.intType)
                {
                    stringBuilder.AppendLine($"public enum {type.Name}:int");
                    stringBuilder.AppendLine("{");
                    var array = Enum.GetValues(type);
                    foreach (var item in array)
                    {
                        var enumString = item.ToString();
                        stringBuilder.AppendLine($"{enumString}={(int)item},");
                    }
                }
                else if (baseType == TouchSocketCoreUtility.longType)
                {
                    stringBuilder.AppendLine($"public enum {type.Name}:long");
                    stringBuilder.AppendLine("{");
                    var array = Enum.GetValues(type);
                    foreach (var item in array)
                    {
                        var enumString = item.ToString();
                        stringBuilder.AppendLine($"{enumString}={(long)item},");
                    }
                }

                stringBuilder.AppendLine("}");
                if (!this.PropertyDic.ContainsKey(type))
                {
                    string className;
                    if (type.GetCustomAttribute<RpcProxyAttribute>() is RpcProxyAttribute attribute)
                    {
                        className = attribute.ClassName ?? type.Name;
                    }
                    else if (CodeGenerator.TryGetProxyTypeName(type, out className))
                    {
                    }
                    else if (this.AllowGen(type.Assembly))
                    {
                        className = type.Name;
                    }
                    else
                    {
                        return;
                    }
                    this.PropertyDic.TryAdd(type, new ClassCellCode() { Name = className, Code = stringBuilder.ToString() });
                }
            }
            else
            {
                string className;
                if (type.GetCustomAttribute<RpcProxyAttribute>() is RpcProxyAttribute attribute)
                {
                    className = attribute.ClassName ?? type.Name;
                }
                else if (CodeGenerator.TryGetProxyTypeName(type, out className))
                {
                }
                else if (this.AllowGen(type.Assembly))
                {
                    className = type.Name;
                }
                else
                {
                    return;
                }
                var stringBuilder = new StringBuilder();

                stringBuilder.AppendLine("");
                if (type.IsStruct())
                {
                    stringBuilder.AppendLine($"public struct {className}");
                }
                else
                {
                    stringBuilder.AppendLine($"public class {className}");
                }

                if (!type.IsStruct() && type.BaseType != typeof(object))
                {
                    this.AddTypeString(type.BaseType, ref deep);
                    if (type.BaseType.IsGenericType)
                    {
                        var types = type.BaseType.GetGenericArguments();
                        foreach (var itemType in types)
                        {
                            this.AddTypeString(itemType, ref deep);
                        }
                        if (m_listType.Contains(type.BaseType.Name))
                        {
                            var typeString = this.GetTypeFullName(types[0]);
                            stringBuilder.Append($":{type.BaseType.Name.Replace("`1", string.Empty)}<{typeString}>");
                        }
                        else if (m_dicType.Contains(type.BaseType.Name))
                        {
                            var keyString = this.GetTypeFullName(types[0]);
                            var valueString = this.GetTypeFullName(types[1]);
                            stringBuilder.Append($": {type.BaseType.Name.Replace("`2", string.Empty)}<{keyString},{valueString}>");
                        }
                    }
                    else if (type.BaseType.IsClass)
                    {
                        stringBuilder.AppendLine($": {this.GetTypeFullName(type.BaseType)}");
                    }
                }

                stringBuilder.AppendLine("{");
                foreach (var itemProperty in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.GetProperty | BindingFlags.SetProperty))
                {
                    this.AddTypeString(itemProperty.PropertyType, ref deep);
                    if (this.PropertyDic.ContainsKey(itemProperty.PropertyType))
                    {
                        stringBuilder.Append($"public {itemProperty.PropertyType.Name} {itemProperty.Name}");
                    }
                    else if (itemProperty.IsNullableType())
                    {
                        stringBuilder.Append($"public {this.GetTypeFullName(itemProperty)}? {itemProperty.Name}");
                    }
                    else if (itemProperty.PropertyType.IsGenericType)
                    {
                        var types = itemProperty.PropertyType.GetGenericArguments();
                        foreach (var itemType in types)
                        {
                            this.AddTypeString(itemType, ref deep);
                        }
                        if (m_listType.Contains(itemProperty.PropertyType.Name))
                        {
                            var typeString = this.GetTypeFullName(types[0]);
                            stringBuilder.Append($"public {itemProperty.PropertyType.Name.Replace("`1", string.Empty)}<{typeString}> {itemProperty.Name}");
                        }
                        else if (m_dicType.Contains(itemProperty.PropertyType.Name))
                        {
                            var keyString = this.GetTypeFullName(types[0]);
                            var valueString = this.GetTypeFullName(types[1]);
                            stringBuilder.Append($"public {itemProperty.PropertyType.Name.Replace("`2", string.Empty)}<{keyString},{valueString}> {itemProperty.Name}");
                        }
                    }
                    else
                    {
                        this.AddTypeString(itemProperty.PropertyType, ref deep);
                        stringBuilder.Append($"public {this.GetTypeFullName(itemProperty.PropertyType)} {itemProperty.Name}");
                    }

                    stringBuilder.AppendLine("{get;set;}");
                }

                foreach (var itemField in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                {
                    this.AddTypeString(itemField.FieldType, ref deep);
                    if (this.PropertyDic.ContainsKey(itemField.FieldType))
                    {
                        stringBuilder.Append($"public {itemField.FieldType.Name} {itemField.Name}");
                    }
                    else if (itemField.IsNullableType())
                    {
                        stringBuilder.Append($"public {this.GetTypeFullName(itemField)}? {itemField.Name}");
                    }
                    else if (itemField.FieldType.IsGenericType)
                    {
                        var types = itemField.FieldType.GetGenericArguments();
                        foreach (var itemType in types)
                        {
                            this.AddTypeString(itemType, ref deep);
                        }
                        if (m_listType.Contains(itemField.FieldType.Name))
                        {
                            var typeString = this.GetTypeFullName(types[0]);
                            stringBuilder.Append($"public {itemField.FieldType.Name.Replace("`1", string.Empty)}<{typeString}> {itemField.Name}");
                        }
                        else if (m_dicType.Contains(itemField.FieldType.Name))
                        {
                            var keyString = this.GetTypeFullName(types[0]);
                            var valueString = this.GetTypeFullName(types[1]);
                            stringBuilder.Append($"public {itemField.FieldType.Name.Replace("`2", string.Empty)}<{keyString},{valueString}> {itemField.Name}");
                        }
                    }
                    else
                    {
                        this.AddTypeString(itemField.FieldType, ref deep);
                        stringBuilder.Append($"public {this.GetTypeFullName(itemField.FieldType)} {itemField.Name}");
                    }

                    stringBuilder.AppendLine(";");
                }

                stringBuilder.AppendLine("}");

                if (!this.PropertyDic.ContainsKey(type))
                {
                    this.PropertyDic.TryAdd(type, new ClassCellCode() { Name = className, Code = stringBuilder.ToString() });
                }
            }
        }

        /// <summary>
        /// 获取类单元参数
        /// </summary>
        /// <returns></returns>
        public ClassCellCode[] GetClassCellCodes()
        {
            return this.PropertyDic.Values.ToArray();
        }

        /// <summary>
        /// 获取类型全名
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetTypeFullName(Type type)
        {
            if (type.FullName == null)
            {
                return type.Name.Replace("&", string.Empty);
            }
            else if (type == typeof(void))
            {
                return null;
            }
            else if (typeof(Task).IsAssignableFrom(type))
            {
                var ts = type.GetGenericArguments();
                return ts.Length == 1 ? ts[0].Name : type.Name;
            }
            else if (type.IsArray)
            {
                var elementType = type.GetElementType();
                return this.GetTypeFullName(elementType) + type.Name.Replace(elementType.Name, string.Empty);
            }
            else if (type.IsNullableType())
            {
                return this.GetTypeFullName(type.GetGenericArguments().Length == 0 ? type : type.GetGenericArguments()[0]);
            }
            else if (type.IsValueTuple())
            {
                var elementTypes = type.GetGenericArguments();

                var stringBuilder = new StringBuilder();
                stringBuilder.Append("(");

                var strings = new List<string>();
                var tupleNames = new List<string>();
                if (this.tupleElementNames != null && this.tupleElementNames.Count > 0)
                {
                    tupleNames.AddRange(this.tupleElementNames.Skip(0).Take(elementTypes.Length));
                    this.tupleElementNames.RemoveRange(0, elementTypes.Length);
                }
                for (var i = 0; i < elementTypes.Length; i++)
                {
                    var item = this.GetTypeFullName(elementTypes[i]);
                    if (tupleNames.Count > 0)
                    {
                        strings.Add($"{item} {tupleNames[i]}");
                    }
                    else
                    {
                        strings.Add($"{item}");
                    }
                }
                //var strs = elementTypes.Select(e => GetTypeFullName(e));

                //foreach (var item in strs)
                //{
                //}
                stringBuilder.Append(string.Join(",", strings));
                stringBuilder.Append(")");
                return stringBuilder.ToString();
            }
            else if (type.IsByRef)
            {
                return this.GetTypeFullName(type.GetElementType());
            }
            else if (type.IsPrimitive || type == typeof(string))
            {
                return type.FullName;
            }
            else if (m_listType.Contains(type.Name))
            {
                var typeInnerString = this.GetTypeFullName(type.GetGenericArguments()[0]);
                var typeString = $"System.Collections.Generic.{type.Name.Replace("`1", string.Empty)}<{typeInnerString}>";
                return typeString;
            }
            else if (m_listType.Contains(type.Name) || m_dicType.Contains(type.Name))
            {
                var keyString = this.GetTypeFullName(type.GetGenericArguments()[0]);
                var valueString = this.GetTypeFullName(type.GetGenericArguments()[1]);
                var typeString = $"System.Collections.Generic.{type.Name.Replace("`2", string.Empty)}<{keyString},{valueString}>";
                return typeString;
            }
            else
            {
                return this.PropertyDic.ContainsKey(type) ? this.PropertyDic[type].Name : type.FullName;
            }
        }

        /// <summary>
        /// 获取类型全名
        /// </summary>
        /// <param name="parameterInfo"></param>
        /// <returns></returns>
        public string GetTypeFullName(ParameterInfo parameterInfo)
        {
            this.tupleElementNames = parameterInfo.ParameterType.FullName.Contains("System.ValueTuple") ? (parameterInfo.GetTupleElementNames()?.ToList()) : default;
            return this.GetTypeFullName(parameterInfo.ParameterType);
        }

        /// <summary>
        /// 获取类型全名
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        public string GetTypeFullName(PropertyInfo propertyInfo)
        {
            this.tupleElementNames = propertyInfo.PropertyType.FullName.Contains("System.ValueTuple") ? (propertyInfo.GetTupleElementNames()?.ToList()) : default;
            return this.GetTypeFullName(propertyInfo.PropertyType);
        }

        /// <summary>
        /// 获取类型全名
        /// </summary>
        /// <param name="fieldInfo"></param>
        /// <returns></returns>
        public string GetTypeFullName(FieldInfo fieldInfo)
        {
            this.tupleElementNames = fieldInfo.FieldType.FullName.Contains("System.ValueTuple") ? (fieldInfo.GetTupleElementNames()?.ToList()) : default;
            return this.GetTypeFullName(fieldInfo.FieldType);
        }

        internal void CheckDeep()
        {
            //foreach (var strItem in GenericTypeDic)
            //{
            //    bool goon = true;
            //    string strItemNew = strItem.Value;
            //    while (goon)
            //    {
            //        goon = false;
            //        foreach (var item in GenericTypeDic.Keys)
            //        {
            //            if (strItemNew.Contains(item.FullName))
            //            {
            //                strItemNew = strItemNew.Replace(item.FullName, item.Name);
            //                goon = true;
            //            }
            //        }
            //    }
            //    GenericTypeDic[strItem.Key] = strItemNew;
            //}

            foreach (var strItem in this.PropertyDic)
            {
                var strItemNew = strItem.Value.Code;
                foreach (var item in this.PropertyDic.Keys)
                {
                    if (strItemNew.Contains(item.FullName))
                    {
                        strItemNew = strItemNew.Replace(item.FullName, item.Name);
                    }
                }
                this.PropertyDic[strItem.Key].Code = strItemNew;
            }
        }

        private bool AllowGen(Assembly assembly)
        {
            foreach (var item in this.Assembly)
            {
                if (assembly == item)
                {
                    return true;
                }
            }
            return false;
        }
    }
}