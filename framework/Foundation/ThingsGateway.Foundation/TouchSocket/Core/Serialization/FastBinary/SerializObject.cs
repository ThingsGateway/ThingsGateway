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
using System.Reflection;

namespace ThingsGateway.Foundation
{
    internal class SerializObject
    {
        private MemberInfo[] m_MemberInfos;
        private FieldInfo[] m_fieldInfos;
        private PropertyInfo[] m_properties;
        public IFastBinaryConverter Converter { get; set; }

        public SerializObject(Type type)
        {
            this.Type = type;
            if (type.IsArray)//数组
            {
                this.InstanceType = InstanceType.Array;
                this.ArrayType = type.GetElementType();
            }
            else if (type.IsClass || type.IsStruct())
            {
                if (type.IsNullableType())
                {
                    type = type.GetGenericArguments()[0];
                }
                this.Type = type;
                if (type.IsList())
                {
                    var genericType = type;
                    while (true)
                    {
                        if (genericType.IsGenericType)
                        {
                            break;
                        }
                        genericType = genericType.BaseType;
                        if (genericType == TouchSocketCoreUtility.objType)
                        {
                            break;
                        }
                    }
                    this.ArgTypes = genericType.GetGenericArguments();
                    this.AddMethod = new Method(type.GetMethod("Add"));
                    this.InstanceType = InstanceType.List;
                }
                else if (type.IsDictionary())
                {
                    var genericType = type;
                    while (true)
                    {
                        if (genericType.IsGenericType)
                        {
                            break;
                        }
                        genericType = genericType.BaseType;
                        if (genericType == TouchSocketCoreUtility.objType)
                        {
                            break;
                        }
                    }
                    this.ArgTypes = genericType.GetGenericArguments();
                    this.AddMethod = new Method(type.GetMethod("Add"));
                    this.InstanceType = InstanceType.Dictionary;
                }
                else
                {
                    this.InstanceType = InstanceType.Class;
                    this.MemberAccessor = new MemberAccessor(type)
                    {
                        OnGetFieldInfes = GetFieldInfos,
                        OnGetProperties = GetProperties
                    };
                    this.MemberAccessor.Build();
                }
                if (type.GetCustomAttribute<FastConverterAttribute>() is FastConverterAttribute attribute)
                {
                    this.Converter = (IFastBinaryConverter)Activator.CreateInstance(attribute.Type);
                }
                this.PropertiesDic = GetProperties(type).ToDictionary(a => a.Name);
                this.FieldInfosDic = GetFieldInfos(type).ToDictionary(a => a.Name);
                if (type.IsGenericType)
                {
                    this.ArgTypes = type.GetGenericArguments();
                }
            }
            this.IsStruct = type.IsStruct();
        }

        public bool IsStruct { get; private set; }

        public Method AddMethod { get; private set; }

        public Type[] ArgTypes { get; private set; }

        public Type ArrayType { get; private set; }

        public FieldInfo[] FieldInfos
        {
            get
            {
                this.m_fieldInfos ??= this.FieldInfosDic.Values.ToArray();
                return this.m_fieldInfos;
            }
        }

        public MemberInfo[] MemberInfos
        {
            get
            {
                if (this.m_MemberInfos == null)
                {
                    var infos = new List<MemberInfo>();
                    infos.AddRange(this.FieldInfosDic.Values);
                    infos.AddRange(this.PropertiesDic.Values);
                    this.m_MemberInfos = infos.ToArray();
                }
                return this.m_MemberInfos;
            }
        }

        public Dictionary<string, FieldInfo> FieldInfosDic { get; private set; }

        public InstanceType InstanceType { get; private set; }

        public MemberAccessor MemberAccessor { get; private set; }

        public PropertyInfo[] Properties
        {
            get
            {
                this.m_properties ??= this.PropertiesDic.Values.ToArray();
                return this.m_properties;
            }
        }

        public Dictionary<string, PropertyInfo> PropertiesDic { get; private set; }

        public Type Type { get; private set; }

        public object GetNewInstance()
        {
            return Activator.CreateInstance(this.Type);
        }

        private static FieldInfo[] GetFieldInfos(Type type)
        {
            return type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Default)
                .Where(p =>
                {
                    return !p.IsInitOnly && (!p.IsDefined(typeof(FastNonSerializedAttribute), true));
                })
                .ToArray();
        }

        private static PropertyInfo[] GetProperties(Type type)
        {
            return type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Default)
                          .Where(p =>
                          {
                              return p.IsDefined(typeof(FastSerializedAttribute), true)
                                  ? true
                                  : p.CanWrite &&
                              p.CanRead &&
                              (!p.IsDefined(typeof(FastNonSerializedAttribute), true) &&
                              (p.SetMethod.GetParameters().Length == 1) &&
                              (p.GetMethod.GetParameters().Length == 0));
                          })
                          .ToArray();
        }
    }
}