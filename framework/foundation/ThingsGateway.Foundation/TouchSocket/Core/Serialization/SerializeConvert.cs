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
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;

#if NET6_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;

#endif

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    /// 高性能序列化器
    /// </summary>

    public static partial class SerializeConvert
    {
#pragma warning disable SYSLIB0011 // 微软觉得不安全，不推荐使用

        #region 普通二进制序列化

        /// <summary>
        /// 普通二进制序列化对象
        /// </summary>
        /// <param name="obj">数据对象</param>
        /// <returns></returns>
        public static byte[] BinarySerialize(in object obj)
        {
            using (var serializeStream = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                bf.Serialize(serializeStream, obj);
                return serializeStream.ToArray();
            }
        }

        /// <summary>
        /// 二进制序列化对象至文件
        /// </summary>
        /// <param name="obj">数据对象</param>
        /// <param name="path">路径</param>
        public static void BinarySerializeToFile(in object obj, string path)
        {
            using (var serializeStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                var bf = new BinaryFormatter();
                bf.Serialize(serializeStream, obj);
                serializeStream.Close();
            }
        }

        /// <summary>
        /// 二进制序列化对象
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="obj"></param>
        public static void BinarySerialize(Stream stream, in object obj)
        {
            var bf = new BinaryFormatter();
            bf.Serialize(stream, obj);
        }

        #endregion 普通二进制序列化

        #region 普通二进制反序列化

        /// <summary>
        /// 从Byte[]中反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="binder"></param>
        /// <returns></returns>
        public static T BinaryDeserialize<T>(byte[] data, int offset, int length, SerializationBinder binder = null)
        {
            using (var DeserializeStream = new MemoryStream(data, offset, length))
            {
                DeserializeStream.Position = 0;
                var bf = new BinaryFormatter();
                if (binder != null)
                {
                    bf.Binder = binder;
                }
                return (T)bf.Deserialize(DeserializeStream);
            }
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="binder"></param>
        /// <returns></returns>
        public static object BinaryDeserialize(byte[] data, int offset, int length, SerializationBinder binder = null)
        {
            using (var DeserializeStream = new MemoryStream(data, offset, length))
            {
                DeserializeStream.Position = 0;
                var bf = new BinaryFormatter();
                if (binder != null)
                {
                    bf.Binder = binder;
                }
                return bf.Deserialize(DeserializeStream);
            }
        }

        /// <summary>
        /// 从Stream中反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="binder"></param>
        /// <returns></returns>
        public static T BinaryDeserialize<T>(Stream stream, SerializationBinder binder = null)
        {
            var bf = new BinaryFormatter();
            if (binder != null)
            {
                bf.Binder = binder;
            }
            return (T)bf.Deserialize(stream);
        }

        /// <summary>
        /// 将二进制文件数据反序列化为指定类型对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public static T BinaryDeserializeFromFile<T>(string path)
        {
            using (var serializeStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var bf = new BinaryFormatter();
                return (T)bf.Deserialize(serializeStream);
            }
        }

        /// <summary>
        /// 将二进制数据反序列化为指定类型对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T BinaryDeserialize<T>(byte[] data)
        {
            return BinaryDeserialize<T>(data, 0, data.Length);
        }

        /// <summary>
        /// 从Byte[]中反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="binder"></param>
        /// <returns></returns>
        public static T BinaryDeserialize<T>(byte[] data, SerializationBinder binder = null)
        {
            return BinaryDeserialize<T>(data, 0, data.Length, binder);
        }

        #endregion 普通二进制反序列化

#pragma warning restore SYSLIB0011 // 微软觉得不安全，不推荐使用

        #region Fast二进制序列化

        /// <summary>
        /// Fast二进制序列化对象
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
#if NET6_0_OR_GREATER
        public static void FastBinarySerialize<[DynamicallyAccessedMembers(FastBinaryFormatter.DynamicallyAccessed)] T>(ByteBlock stream, [DynamicallyAccessedMembers(FastBinaryFormatter.DynamicallyAccessed)] in T obj)
#else

        public static void FastBinarySerialize<T>(ByteBlock stream, in T obj)
#endif
        {
            FastBinaryFormatter.Serialize(stream, obj);
        }

        /// <summary>
        /// Fast二进制序列化对象
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
#if NET6_0_OR_GREATER
        public static byte[] FastBinarySerialize<[DynamicallyAccessedMembers(FastBinaryFormatter.DynamicallyAccessed)] T>([DynamicallyAccessedMembers(FastBinaryFormatter.DynamicallyAccessed)] in T obj)
#else

        public static byte[] FastBinarySerialize<T>(in T obj)
#endif
        {
            using (var byteBlock = new ByteBlock())
            {
                FastBinarySerialize(byteBlock, obj);
                return byteBlock.ToArray();
            }
        }

        #endregion Fast二进制序列化

        #region Fast二进制反序列化

        /// <summary>
        /// Fast反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
#if NET6_0_OR_GREATER
        public static T FastBinaryDeserialize<[DynamicallyAccessedMembers(FastBinaryFormatter.DynamicallyAccessed)] T>(byte[] data, int offset)
#else

        public static T FastBinaryDeserialize<T>(byte[] data, int offset)
#endif
        {
            return (T)FastBinaryFormatter.Deserialize(data, offset, typeof(T));
        }

        /// <summary>
        /// Fast反序列化
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="type"></param>
        /// <returns></returns>
#if NET6_0_OR_GREATER
        public static object FastBinaryDeserialize(byte[] data, int offset, [DynamicallyAccessedMembers(FastBinaryFormatter.DynamicallyAccessed)] Type type)
#else

        public static object FastBinaryDeserialize(byte[] data, int offset, Type type)
#endif
        {
            return FastBinaryFormatter.Deserialize(data, offset, type);
        }

        /// <summary>
        /// 从Byte[]中反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
#if NET6_0_OR_GREATER
        public static T FastBinaryDeserialize<[DynamicallyAccessedMembers(FastBinaryFormatter.DynamicallyAccessed)] T>(byte[] data)
#else

        public static T FastBinaryDeserialize<T>(byte[] data)
#endif
        {
            return FastBinaryDeserialize<T>(data, 0);
        }

        #endregion Fast二进制反序列化

        #region Xml序列化和反序列化

        /// <summary>
        /// Xml序列化数据对象
        /// </summary>
        /// <param name="obj">数据对象</param>
        /// <param name="encoding">编码格式</param>
        /// <returns></returns>
        public static string XmlSerializeToString(object obj, Encoding encoding)
        {
            return encoding.GetString(XmlSerializeToBytes(obj));
        }

        /// <summary>
        /// Xml序列化数据对象
        /// </summary>
        /// <param name="obj">数据对象</param>
        /// <returns></returns>
        public static string XmlSerializeToString(object obj)
        {
            return XmlSerializeToString(obj, Encoding.UTF8);
        }

        /// <summary>
        /// Xml序列化数据对象
        /// </summary>
        /// <param name="obj">数据对象</param>
        /// <returns></returns>
        public static byte[] XmlSerializeToBytes(object obj)
        {
            using (var fileStream = new MemoryStream())
            {
                var xml = new XmlSerializer(obj.GetType());
                xml.Serialize(fileStream, obj);
                return fileStream.ToArray();
            }
        }

        /// <summary>
        /// Xml序列化至文件
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="path"></param>
        public static void XmlSerializeToFile(object obj, string path)
        {
            using (var fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                var xml = new XmlSerializer(obj.GetType());
                xml.Serialize(fileStream, obj);
                fileStream.Close();
            }
        }

        /// <summary>
        /// Xml反序列化
        /// </summary>
        /// <typeparam name="T">反序列化类型</typeparam>
        /// <param name="datas">数据</param>
        /// <returns></returns>
        public static T XmlDeserializeFromBytes<T>(byte[] datas)
        {
            var xmlserializer = new XmlSerializer(typeof(T));
            using (Stream xmlstream = new MemoryStream(datas))
            {
                return (T)xmlserializer.Deserialize(xmlstream);
            }
        }

        /// <summary>
        /// Xml反序列化
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object XmlDeserializeFromBytes(byte[] datas, Type type)
        {
            var xmlserializer = new XmlSerializer(type);
            using (Stream xmlstream = new MemoryStream(datas))
            {
                return xmlserializer.Deserialize(xmlstream);
            }
        }

        /// <summary>
        /// Xml反序列化
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="xmlString">xml字符串</param>
        /// <param name="encoding">编码格式</param>
        /// <returns></returns>
        public static T XmlDeserializeFromString<T>(string xmlString, Encoding encoding)
        {
            var xmlserializer = new XmlSerializer(typeof(T));
            using (Stream xmlstream = new MemoryStream(encoding.GetBytes(xmlString)))
            {
                return (T)xmlserializer.Deserialize(xmlstream);
            }
        }

        /// <summary>
        /// Xml反序列化
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="json">xml字符串</param>
        /// <returns></returns>
        public static T XmlDeserializeFromString<T>(string json)
        {
            return XmlDeserializeFromString<T>(json, Encoding.UTF8);
        }

        /// <summary>
        /// Xml反序列化
        /// </summary>
        /// <typeparam name="T">反序列化类型</typeparam>
        /// <param name="path">文件路径</param>
        /// <returns></returns>
        public static T XmlDeserializeFromFile<T>(string path)
        {
            using (Stream xmlstream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var xmlserializer = new XmlSerializer(typeof(T));
                return (T)xmlserializer.Deserialize(xmlstream);
            }
        }

        #endregion Xml序列化和反序列化

        #region Json序列化和反序列化

        /// <summary>
        /// 转换为Json
        /// </summary>
        /// <param name="item"></param>
        /// <param name="isIndented"></param>
        /// <returns></returns>
        public static string ToJsonString(this object item, bool isIndented = false)
        {
            if (isIndented)
                return Newtonsoft.Json.JsonConvert.SerializeObject(item, Newtonsoft.Json.Formatting.Indented);
            else
                return Newtonsoft.Json.JsonConvert.SerializeObject(item);
        }

        /// <summary>
        /// 从字符串到json
        /// </summary>
        /// <param name="json"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object FromJsonString(this string json, Type type)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject(json, type);
        }

        /// <summary>
        /// 从字符串到json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T FromJsonString<T>(this string json)
        {
            return (T)FromJsonString(json, typeof(T));
        }

        /// <summary>
        /// Json序列化数据对象
        /// </summary>
        /// <param name="obj">数据对象</param>
        /// <returns></returns>
        public static byte[] JsonSerializeToBytes(object obj)
        {
            return Encoding.UTF8.GetBytes(ToJsonString(obj));
        }

        /// <summary>
        /// Json序列化至文件
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="path"></param>
        public static void JsonSerializeToFile(object obj, string path)
        {
            using (var fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                var date = JsonSerializeToBytes(obj);
                fileStream.Write(date, 0, date.Length);
                fileStream.Close();
            }
        }

        /// <summary>
        /// Json反序列化
        /// </summary>
        /// <typeparam name="T">反序列化类型</typeparam>
        /// <param name="datas">数据</param>
        /// <returns></returns>
        public static T JsonDeserializeFromBytes<T>(byte[] datas)
        {
            return (T)JsonDeserializeFromBytes(datas, typeof(T));
        }

        /// <summary>
        /// Json反序列化
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object JsonDeserializeFromBytes(byte[] datas, Type type)
        {
            return FromJsonString(Encoding.UTF8.GetString(datas), type);
        }

        /// <summary>
        /// Json反序列化
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="json">json字符串</param>
        /// <returns></returns>
        public static T JsonDeserializeFromString<T>(string json)
        {
            return FromJsonString<T>(json);
        }

        /// <summary>
        /// Json反序列化
        /// </summary>
        /// <typeparam name="T">反序列化类型</typeparam>
        /// <param name="path">文件路径</param>
        /// <returns></returns>
        public static T JsonDeserializeFromFile<T>(string path)
        {
            return JsonDeserializeFromString<T>(File.ReadAllText(path));
        }

        #endregion Json序列化和反序列化
    }
}