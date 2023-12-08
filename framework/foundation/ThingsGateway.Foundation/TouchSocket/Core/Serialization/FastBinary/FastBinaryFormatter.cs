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
using System.Collections;
using System.Collections.Concurrent;
using System.Data;
using System.Text;

#if NET6_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    /// 快速二进制序列化。
    /// </summary>
    public static partial class FastBinaryFormatter
    {
#if NET6_0_OR_GREATER
        /// <summary>
        /// DynamicallyAccessed
        /// </summary>
        public const DynamicallyAccessedMemberTypes DynamicallyAccessed = DynamicallyAccessedMemberTypes.All;
#endif

        static FastBinaryFormatter()
        {
            AddFastBinaryConverter(typeof(string), new StringFastBinaryConverter());
            AddFastBinaryConverter(typeof(Version), new VersionFastBinaryConverter());
            AddFastBinaryConverter(typeof(ByteBlock), new ByteBlockFastBinaryConverter());
            AddFastBinaryConverter(typeof(MemoryStream), new MemoryStreamFastBinaryConverter());
            AddFastBinaryConverter(typeof(Guid), new GuidFastBinaryConverter());
            AddFastBinaryConverter(typeof(DataTable), new DataTableFastBinaryConverter());
            AddFastBinaryConverter(typeof(DataSet), new DataSetFastBinaryConverter());
        }

        private static readonly ConcurrentDictionary<Type, SerializObject> m_instanceCache = new ConcurrentDictionary<Type, SerializObject>();

        /// <summary>
        /// 添加转换器。
        /// </summary>
#if NET6_0_OR_GREATER
        public static void AddFastBinaryConverter<[DynamicallyAccessedMembers(DynamicallyAccessed)] TType, [DynamicallyAccessedMembers(DynamicallyAccessed)] TConverter>() where TConverter : IFastBinaryConverter, new()
#else

        public static void AddFastBinaryConverter<TType, TConverter>() where TConverter : IFastBinaryConverter, new()
#endif

        {
            AddFastBinaryConverter(typeof(TType), (IFastBinaryConverter)Activator.CreateInstance(typeof(TConverter)));
        }

        /// <summary>
        /// 添加转换器。
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="converter"></param>
#if NET6_0_OR_GREATER
        public static void AddFastBinaryConverter<[DynamicallyAccessedMembers(DynamicallyAccessed)] TType>(IFastBinaryConverter converter)
#else

        public static void AddFastBinaryConverter<TType>(IFastBinaryConverter converter)
#endif
        {
            AddFastBinaryConverter(typeof(TType), converter);
        }

        /// <summary>
        /// 添加转换器。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="converter"></param>
#if NET6_0_OR_GREATER
        public static void AddFastBinaryConverter([DynamicallyAccessedMembers(DynamicallyAccessed)] Type type, IFastBinaryConverter converter)
#else

        public static void AddFastBinaryConverter(Type type, IFastBinaryConverter converter)
#endif
        {
            var serializObject = new SerializObject(type, converter);
            m_instanceCache.AddOrUpdate(type, serializObject, (k, v) => serializObject);
        }

        #region Serialize

        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <param name="byteBlock">流</param>
        /// <param name="graph">对象</param>
#if NET6_0_OR_GREATER
        public static void Serialize<[DynamicallyAccessedMembers(DynamicallyAccessed)] T>(ByteBlock byteBlock, [DynamicallyAccessedMembers(DynamicallyAccessed)] in T graph)
#else

        public static void Serialize<T>(ByteBlock byteBlock, in T graph)
#endif
        {
            byteBlock.Position = 1;
            SerializeObject(byteBlock, graph);
            byteBlock.Buffer[0] = 1;
            byteBlock.SetLength(byteBlock.Position);
        }

        private static int SerializeClass<T>(ByteBlock stream, T obj, Type type)
        {
            var len = 0;
            if (obj != null)
            {
                var serializObject = GetOrAddInstance(type);

                for (var i = 0; i < serializObject.MemberInfos.Length; i++)
                {
                    var memberInfo = serializObject.MemberInfos[i];
                    var propertyBytes = Encoding.UTF8.GetBytes(memberInfo.Name);
                    if (propertyBytes.Length > byte.MaxValue)
                    {
                        throw new Exception($"属性名：{memberInfo.Name}超长");
                    }
                    stream.Write((byte)propertyBytes.Length);
                    stream.Write(propertyBytes, 0, propertyBytes.Length);
                    len += propertyBytes.Length + 1;
                    //len += SerializeObject(stream, property.GetValue(obj));
                    len += SerializeObject(stream, serializObject.MemberAccessor.GetValue(obj, memberInfo.Name));
                }
                //foreach (PropertyInfo property in serializObject.Properties)
                //{
                //}

                //foreach (FieldInfo fieldInfo in serializObject.FieldInfos)
                //{
                //    byte[] propertyBytes = Encoding.UTF8.GetBytes(fieldInfo.Name);
                //    if (propertyBytes.Length > byte.MaxValue)
                //    {
                //        throw new Exception($"属性名：{fieldInfo.Name}超长");
                //    }
                //    byte lenBytes = (byte)propertyBytes.Length;
                //    stream.Write(lenBytes);
                //    stream.Write(propertyBytes, 0, propertyBytes.Length);
                //    len += propertyBytes.Length + 1;
                //    len += SerializeObject(stream, fieldInfo.GetValue(obj));
                //}
            }
            return len;
        }

        private static int SerializeDictionary(in ByteBlock stream, in IEnumerable param)
        {
            var len = 0;
            if (param != null)
            {
                var oldPosition = stream.Position;
                stream.Position += 4;
                len += 4;
                uint paramLen = 0;

                foreach (var item in param)
                {
                    len += SerializeObject(stream, DynamicMethodMemberAccessor.Default.GetValue(item, "Key"));
                    len += SerializeObject(stream, DynamicMethodMemberAccessor.Default.GetValue(item, "Value"));
                    paramLen++;
                }
                var newPosition = stream.Position;
                stream.Position = oldPosition;
                stream.Write(TouchSocketBitConverter.Default.GetBytes(paramLen));
                stream.Position = newPosition;
            }
            return len;
        }

        private static int SerializeIListOrArray(in ByteBlock stream, in IEnumerable param)
        {
            var len = 0;
            if (param != null)
            {
                var oldPosition = stream.Position;
                stream.Position += 4;
                len += 4;
                uint paramLen = 0;

                foreach (var item in param)
                {
                    paramLen++;
                    len += SerializeObject(stream, item);
                }
                var newPosition = stream.Position;
                stream.Position = oldPosition;
                stream.Write(TouchSocketBitConverter.Default.GetBytes(paramLen));
                stream.Position = newPosition;
            }
            return len;
        }

        private static int SerializeObject<T>(in ByteBlock byteBlock, in T graph)
        {
            var len = 0;
            byte[] data = null;

            var startPosition = byteBlock.Position;
            long endPosition;
            if (graph != null)
            {
                var type = graph.GetType();
                if (type.IsPrimitive)
                {
                    switch (graph)
                    {
                        case byte value:
                            {
                                data = new byte[] { value };
                                break;
                            }
                        case sbyte value:
                            {
                                data = TouchSocketBitConverter.Default.GetBytes(value);
                                break;
                            }
                        case bool value:
                            {
                                data = TouchSocketBitConverter.Default.GetBytes(value);
                                break;
                            }
                        case short value:
                            {
                                data = TouchSocketBitConverter.Default.GetBytes(value);
                                break;
                            }
                        case ushort value:
                            {
                                data = TouchSocketBitConverter.Default.GetBytes(value);
                                break;
                            }
                        case int value:
                            {
                                data = TouchSocketBitConverter.Default.GetBytes(value);
                                break;
                            }
                        case uint value:
                            {
                                data = TouchSocketBitConverter.Default.GetBytes(value);
                                break;
                            }
                        case long value:
                            {
                                data = TouchSocketBitConverter.Default.GetBytes(value);
                                break;
                            }
                        case ulong value:
                            {
                                data = TouchSocketBitConverter.Default.GetBytes(value);
                                break;
                            }
                        case float value:
                            {
                                data = TouchSocketBitConverter.Default.GetBytes(value);
                                break;
                            }
                        case double value:
                            {
                                data = TouchSocketBitConverter.Default.GetBytes(value);
                                break;
                            }
                        case char value:
                            {
                                data = TouchSocketBitConverter.Default.GetBytes(value);
                                break;
                            }
                        default:
                            {
                                throw new Exception("未知基础类型");
                            }
                    }
                }
                else
                {
                    switch (graph)
                    {
                        //case string value:
                        //    {
                        //        data = Encoding.UTF8.GetBytes(value);
                        //        break;
                        //    }
                        case decimal value:
                            {
                                data = TouchSocketBitConverter.Default.GetBytes(value);
                                break;
                            }
                        case DateTime value:
                            {
                                data = TouchSocketBitConverter.Default.GetBytes(value.Ticks);
                                break;
                            }
                        case Enum _:
                            {
                                var enumValType = Enum.GetUnderlyingType(type);

                                if (enumValType == TouchSocketCoreUtility.byteType)
                                {
                                    data = new byte[] { Convert.ToByte(graph) };
                                }
                                else
                                {
                                    if (enumValType == TouchSocketCoreUtility.shortType)
                                    {
                                        data = TouchSocketBitConverter.Default.GetBytes(Convert.ToInt16(graph));
                                    }
                                    else
                                    {
                                        if (enumValType == TouchSocketCoreUtility.intType)
                                        {
                                            data = TouchSocketBitConverter.Default.GetBytes(Convert.ToInt32(graph));
                                        }
                                        else
                                        {
                                            data = TouchSocketBitConverter.Default.GetBytes(Convert.ToInt64(graph));
                                        }
                                    }
                                }
                                break;
                            }
                        case byte[] value:
                            {
                                data = value;
                                break;
                            }
                        default:
                            {
                                byteBlock.Position += 4;
                                var serializeObj = GetOrAddInstance(type);
                                if (serializeObj.Converter != null)
                                {
                                    len += serializeObj.Converter.Write(byteBlock, graph);
                                }
                                else
                                {
                                    switch (serializeObj.InstanceType)
                                    {
                                        case InstanceType.List:
                                            len += SerializeIListOrArray(byteBlock, (IEnumerable)graph);
                                            break;

                                        case InstanceType.Array:
                                            len += SerializeIListOrArray(byteBlock, (IEnumerable)graph);
                                            break;

                                        case InstanceType.Dictionary:
                                            len += SerializeDictionary(byteBlock, (IEnumerable)graph);
                                            break;

                                        default:
                                        case InstanceType.Class:
                                            len += SerializeClass(byteBlock, graph, type);
                                            break;
                                    }
                                }
                                break;
                            }
                    }
                }

                if (data != null)
                {
                    len = data.Length;
                    endPosition = len + startPosition + 4;
                }
                else
                {
                    endPosition = byteBlock.Position;
                }
            }
            else
            {
                endPosition = startPosition + 4;
            }

            var lenBuffer = TouchSocketBitConverter.Default.GetBytes(len);
            byteBlock.Position = startPosition;
            byteBlock.Write(lenBuffer, 0, lenBuffer.Length);

            if (data != null)
            {
                byteBlock.Write(data, 0, data.Length);
            }
            byteBlock.Position = endPosition;
            return len + 4;
        }

        #endregion Serialize

        #region Deserialize

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="type"></param>
        /// <returns></returns>
#if NET6_0_OR_GREATER
        public static object Deserialize(byte[] data, int offset, [DynamicallyAccessedMembers(DynamicallyAccessed)] Type type)
#else

        public static object Deserialize(byte[] data, int offset, Type type)
#endif
        {
            if (data[offset] != 1)
            {
                throw new Exception("Fast反序列化数据流解析错误。");
            }
            offset += 1;
            return Deserialize(type, data, ref offset);
        }

        private static object Deserialize(Type type, byte[] datas, ref int offset)
        {
            var nullable = type.IsNullableType();
            if (nullable)
            {
                type = type.GenericTypeArguments[0];
            }
            object obj;
            var len = TouchSocketBitConverter.Default.ToInt32(datas, offset);
            offset += 4;
            if (len > 0)
            {
                //if (type == TouchSocketCoreUtility.stringType)
                //{
                //    obj = Encoding.UTF8.GetString(datas, offset, len);
                //}
                //else
                if (type == TouchSocketCoreUtility.byteType)
                {
                    obj = datas[offset];
                }
                else if (type == TouchSocketCoreUtility.sbyteType)
                {
                    obj = (sbyte)(TouchSocketBitConverter.Default.ToInt16(datas, offset));
                }
                else if (type == TouchSocketCoreUtility.boolType)
                {
                    obj = (TouchSocketBitConverter.Default.ToBoolean(datas, offset));
                }
                else if (type == TouchSocketCoreUtility.shortType)
                {
                    obj = (TouchSocketBitConverter.Default.ToInt16(datas, offset));
                }
                else if (type == TouchSocketCoreUtility.ushortType)
                {
                    obj = (TouchSocketBitConverter.Default.ToUInt16(datas, offset));
                }
                else if (type == TouchSocketCoreUtility.intType)
                {
                    obj = (TouchSocketBitConverter.Default.ToInt32(datas, offset));
                }
                else if (type == TouchSocketCoreUtility.uintType)
                {
                    obj = (TouchSocketBitConverter.Default.ToUInt32(datas, offset));
                }
                else if (type == TouchSocketCoreUtility.longType)
                {
                    obj = (TouchSocketBitConverter.Default.ToInt64(datas, offset));
                }
                else if (type == TouchSocketCoreUtility.ulongType)
                {
                    obj = (TouchSocketBitConverter.Default.ToUInt64(datas, offset));
                }
                else if (type == TouchSocketCoreUtility.floatType)
                {
                    obj = (TouchSocketBitConverter.Default.ToSingle(datas, offset));
                }
                else if (type == TouchSocketCoreUtility.doubleType)
                {
                    obj = (TouchSocketBitConverter.Default.ToDouble(datas, offset));
                }
                else if (type == TouchSocketCoreUtility.decimalType)
                {
                    obj = (TouchSocketBitConverter.Default.ToDecimal(datas, offset));
                }
                else if (type == TouchSocketCoreUtility.charType)
                {
                    obj = (TouchSocketBitConverter.Default.ToChar(datas, offset));
                }
                else if (type == TouchSocketCoreUtility.dateTimeType)
                {
                    obj = (new DateTime(TouchSocketBitConverter.Default.ToInt64(datas, offset)));
                }
                else if (type.BaseType == typeof(Enum))
                {
                    var enumType = Enum.GetUnderlyingType(type);

                    if (enumType == typeof(byte))
                    {
                        obj = Enum.ToObject(type, datas[offset]);
                    }
                    else
                    {
                        if (enumType == typeof(short))
                        {
                            obj = Enum.ToObject(type, TouchSocketBitConverter.Default.ToInt16(datas, offset));
                        }
                        else
                        {
                            if (enumType == typeof(int))
                            {
                                obj = Enum.ToObject(type, TouchSocketBitConverter.Default.ToInt32(datas, offset));
                            }
                            else
                            {
                                obj = Enum.ToObject(type, TouchSocketBitConverter.Default.ToInt64(datas, offset));
                            }
                        }
                    }
                }
                else if (type == TouchSocketCoreUtility.bytesType)
                {
                    var data = new byte[len];
                    Array.Copy(datas, offset, data, 0, len);
                    obj = data;
                }
                else if (type.IsClass || type.IsStruct())
                {
                    var serializeObj = GetOrAddInstance(type);
                    if (serializeObj.Converter != null)
                    {
                        obj = serializeObj.Converter.Read(datas, offset, len);
                    }
                    else
                    {
                        obj = DeserializeClass(type, datas, offset, len);
                    }
                }
                else
                {
                    throw new Exception("未定义的类型：" + type.ToString());
                }
            }
            else
            {
                if (nullable)
                {
                    obj = null;
                }
                else
                {
                    obj = type.GetDefault();
                }
            }
            offset += len;
            return obj;
        }

        private static object DeserializeClass(Type type, byte[] datas, int offset, int length)
        {
            var serializObject = GetOrAddInstance(type);

            object instance;
            switch (serializObject.InstanceType)
            {
                case InstanceType.Class:
                    {
                        instance = serializObject.GetNewInstance();
                        var index = offset;
                        while (offset - index < length && (length >= 4))
                        {
                            int len = datas[offset];
                            var propertyName = Encoding.UTF8.GetString(datas, offset + 1, len);
                            offset += len + 1;
                            if (serializObject.IsStruct)
                            {
                                if (serializObject.PropertiesDic.ContainsKey(propertyName))
                                {
                                    var property = serializObject.PropertiesDic[propertyName];
                                    var obj = Deserialize(property.PropertyType, datas, ref offset);
                                    property.SetValue(instance, obj);
                                }
                                else if (serializObject.FieldInfosDic.ContainsKey(propertyName))
                                {
                                    var property = serializObject.FieldInfosDic[propertyName];
                                    var obj = Deserialize(property.FieldType, datas, ref offset);
                                    property.SetValue(instance, obj);
                                }
                                else
                                {
                                    var pLen = TouchSocketBitConverter.Default.ToInt32(datas, offset);
                                    offset += 4;
                                    offset += pLen;
                                }
                            }
                            else
                            {
                                if (serializObject.PropertiesDic.TryGetValue(propertyName, out var property))
                                {
                                    var obj = Deserialize(property.PropertyType, datas, ref offset);
                                    serializObject.MemberAccessor.SetValue(instance, property.Name, obj);
                                }
                                else if (serializObject.FieldInfosDic.TryGetValue(propertyName, out var fieldInfo))
                                {
                                    var obj = Deserialize(fieldInfo.FieldType, datas, ref offset);
                                    serializObject.MemberAccessor.SetValue(instance, fieldInfo.Name, obj);
                                }
                                else
                                {
                                    var pLen = TouchSocketBitConverter.Default.ToInt32(datas, offset);
                                    offset += 4;
                                    offset += pLen;
                                }
                            }
                        }
                        break;
                    }
                case InstanceType.List:
                    {
                        instance = serializObject.GetNewInstance();
                        if (length > 0)
                        {
                            var paramLen = TouchSocketBitConverter.Default.ToUInt32(datas, offset);
                            offset += 4;
                            for (uint i = 0; i < paramLen; i++)
                            {
                                var obj = Deserialize(serializObject.ArgTypes[0], datas, ref offset);
                                serializObject.AddMethod.Invoke(instance, new object[] { obj });
                            }
                        }
                        else
                        {
                            instance = null;
                        }
                        break;
                    }
                case InstanceType.Array:
                    {
                        if (length > 0)
                        {
                            var paramLen = TouchSocketBitConverter.Default.ToUInt32(datas, offset);
                            var array = Array.CreateInstance(serializObject.ArrayType, paramLen);

                            offset += 4;
                            for (uint i = 0; i < paramLen; i++)
                            {
                                var obj = Deserialize(serializObject.ArrayType, datas, ref offset);
                                array.SetValue(obj, i);
                            }
                            instance = array;
                        }
                        else
                        {
                            instance = null;
                        }
                        break;
                    }
                case InstanceType.Dictionary:
                    {
                        instance = serializObject.GetNewInstance();
                        if (length > 0)
                        {
                            var paramLen = TouchSocketBitConverter.Default.ToUInt32(datas, offset);
                            offset += 4;
                            for (uint i = 0; i < paramLen; i++)
                            {
                                var key = Deserialize(serializObject.ArgTypes[0], datas, ref offset);
                                var value = Deserialize(serializObject.ArgTypes[1], datas, ref offset);
                                if (key != null)
                                {
                                    serializObject.AddMethod.Invoke(instance, new object[] { key, value });
                                }
                            }

                            //uint paramLen = TouchSocketBitConverter.Default.ToUInt32(datas, offset);
                            //offset += 4;
                            //for (uint i = 0; i < paramLen; i++)
                            //{
                            //    offset += 4;
                            //    offset += datas[offset] + 1;
                            //    object key = this.Deserialize(instanceObject.ArgTypes[0], datas, ref offset);

                            //    offset += datas[offset] + 1;
                            //    object value = this.Deserialize(instanceObject.ArgTypes[1], datas, ref offset);
                            //    if (key != null)
                            //    {
                            //        instanceObject.AddMethod.Invoke(instance, new object[] { key, value });
                            //    }
                            //}
                        }
                        else
                        {
                            instance = null;
                        }
                        break;
                    }
                default:
                    instance = null;
                    break;
            }

            return instance;
        }

        #endregion Deserialize

        private static SerializObject GetOrAddInstance(Type type)
        {
            if (type.IsNullableType())
            {
                type = type.GetGenericArguments()[0];
            }

            if (m_instanceCache.TryGetValue(type, out var instance))
            {
                return instance;
            }

            if (type.IsArray || type.IsClass || type.IsStruct())
            {
                var instanceObject = new SerializObject(type);
                m_instanceCache.TryAdd(type, instanceObject);
                return instanceObject;
            }
            return null;
        }
    }
}