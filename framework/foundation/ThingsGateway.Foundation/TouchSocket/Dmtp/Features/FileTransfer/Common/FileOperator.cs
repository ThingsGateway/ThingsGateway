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

namespace ThingsGateway.Foundation.Dmtp.FileTransfer
{
    /// <summary>
    /// 文件传输操作器。
    /// </summary>
    public class FileOperator : FlowOperator
    {
        /// <summary>
        /// 文件分块大小，默认512*1024字节。
        /// 不要超过1024*1024字节。
        /// </summary>
        public int FileSectionSize { get; set; } = 512 * 1024;

        /// <summary>
        /// 文件资源信息。此值不为空时复用，可能会尝试断点续传。
        /// </summary>
        public FileResourceInfo ResourceInfo { get; set; }

        /// <summary>
        /// 资源文件路径，
        /// 可输入绝对路径，也可以输入相对路径。
        /// </summary>
        public string ResourcePath { get; set; }

        /// <summary>
        /// 存放路径，
        /// 可输入绝对路径，也可以输入相对路径。
        /// 但是必须包含文件名及扩展名。
        /// </summary>
        public string SavePath { get; set; }

        /// <summary>
        /// 失败重试次数。默认10。
        /// </summary>
        public int TryCount { get; set; } = 10;

        internal Result SetResult(Result result)
        {
            this.Result = result;
            return result;
        }

        internal Task AddFlowAsync(int flow)
        {
            return this.ProtectedAddFlowAsync(flow);
        }

        internal void AddCompletedLength(int flow)
        {
            this.completedLength += flow;
        }

        /// <summary>
        /// 设置流长度
        /// </summary>
        /// <param name="len"></param>
        internal void SetLength(long len)
        {
            this.Length = len;
        }
    }
}