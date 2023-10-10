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

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    /// 常量
    /// </summary>
    public class TouchSocketCoreUtility
    {
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public static readonly Type stringType = typeof(string);
        public static readonly Type byteType = typeof(byte);
        public static readonly Type sbyteType = typeof(sbyte);
        public static readonly Type shortType = typeof(short);
        public static readonly Type objType = typeof(object);
        public static readonly Type ushortType = typeof(ushort);
        public static readonly Type intType = typeof(int);
        public static readonly Type uintType = typeof(uint);
        public static readonly Type boolType = typeof(bool);
        public static readonly Type charType = typeof(char);
        public static readonly Type longType = typeof(long);
        public static readonly Type ulongType = typeof(ulong);
        public static readonly Type floatType = typeof(float);
        public static readonly Type doubleType = typeof(double);
        public static readonly Type decimalType = typeof(decimal);
        public static readonly Type dateTimeType = typeof(DateTime);
        public static readonly Type bytesType = typeof(byte[]);
        public static readonly Type dicType = typeof(IDictionary);
        public static readonly Type arrayType = typeof(Array);
        public static readonly Type nullableType = typeof(Nullable<>);

        public static readonly byte[] ZeroBytes = new byte[0];
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释

        /// <summary>
        /// 空字符串常亮
        /// </summary>
        public const string Empty = "";
    }
}