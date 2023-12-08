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
using System.Collections.Specialized;

namespace ThingsGateway.Foundation.Http
{
    /// <summary>
    /// 表单文件
    /// </summary>
    public interface IFormFile
    {
        /// <summary>
        /// 获取Content-Disposition
        /// </summary>
        string ContentDisposition { get; }

        /// <summary>
        /// 获取Content-Type
        /// </summary>
        string ContentType { get; }

        /// <summary>
        /// 实际的数据
        /// </summary>
        public byte[] Data { get; }

        /// <summary>
        /// 数据对
        /// </summary>
        public NameValueCollection DataPair { get; }

        /// <summary>
        /// 获取file name
        /// </summary>
        string FileName { get; }

        /// <summary>
        /// 文件长度。在数据接收完成之前，该值为-1;
        /// </summary>
        long Length { get; }

        /// <summary>
        ///  获取name字段
        /// </summary>
        string Name { get; }

        ///// <summary>
        ///// 读取文件数据  //太麻烦先不实现
        ///// </summary>
        //public int Read(byte[] buffer, int offset, int count);
    }

    internal class InternalFormFile : IFormFile
    {
        public string ContentDisposition => this.DataPair["Content-Disposition"];

        public string ContentType => this.DataPair["Content-Type"];

        public byte[] Data { get; set; }
        public NameValueCollection DataPair { get; set; }
        public string FileName => this.DataPair["filename"];
        public long Length => this.Data == null ? 0 : this.Data.Length;

        public string Name => this.DataPair["name"];
        //public int Read(byte[] buffer, int offset, int count)
        //{
        //    return this.ReadAction(buffer, offset, count);
        //}

        //public Func<byte[], int, int, int> ReadAction { get; set; }
    }
}