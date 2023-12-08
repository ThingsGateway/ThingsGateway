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

using System.Text;

namespace ThingsGateway.Foundation.Dmtp.Rpc
{
    /// <summary>
    /// 默认序列化选择器
    /// </summary>
    public class DefaultSerializationSelector : SerializationSelector
    {
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="serializationType"></param>
        /// <param name="parameterBytes"></param>
        /// <param name="parameterType"></param>
        /// <returns></returns>
        public override object DeserializeParameter(SerializationType serializationType, byte[] parameterBytes, Type parameterType)
        {
            if (parameterBytes == null)
            {
                return parameterType.GetDefault();
            }
            switch (serializationType)
            {
                case SerializationType.FastBinary:
                    {
                        return SerializeConvert.FastBinaryDeserialize(parameterBytes, 0, parameterType);
                    }
                case SerializationType.SystemBinary:
                    {
                        return SerializeConvert.BinaryDeserialize(parameterBytes, 0, parameterBytes.Length);
                    }
                case SerializationType.Json:
                    {
                        return SerializeConvert.FromJsonString(Encoding.UTF8.GetString(parameterBytes), parameterType);
                    }
                case SerializationType.Xml:
                    {
                        return SerializeConvert.XmlDeserializeFromBytes(parameterBytes, parameterType);
                    }
                default:
                    throw new RpcException("未指定的反序列化方式");
            }
        }

        /// <summary>
        /// 序列化参数
        /// </summary>
        /// <param name="serializationType"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public override byte[] SerializeParameter(SerializationType serializationType, object parameter)
        {
            if (parameter == null)
            {
                return null;
            }
            switch (serializationType)
            {
                case SerializationType.FastBinary:
                    {
                        return SerializeConvert.FastBinarySerialize(parameter);
                    }
                case SerializationType.SystemBinary:
                    {
                        return SerializeConvert.BinarySerialize(parameter);
                    }
                case SerializationType.Json:
                    {
                        return SerializeConvert.JsonSerializeToBytes(parameter);
                    }
                case SerializationType.Xml:
                    {
                        return SerializeConvert.XmlSerializeToBytes(parameter);
                    }
                default:
                    throw new RpcException("未指定的序列化方式");
            }
        }
    }
}