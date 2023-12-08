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

namespace ThingsGateway.Foundation.Dmtp
{
    /// <summary>
    /// 提供一个基于Dmtp协议的，可以独立读写的通道。
    /// </summary>
    public partial interface IDmtpChannel : IDisposable, IEnumerable<ByteBlock>
    {
        /// <summary>
        /// 通道传输速度限制
        /// </summary>
        long MaxSpeed { get; set; }

        /// <summary>
        /// 具有可读数据的长度
        /// </summary>
        long Available { get; }

        /// <summary>
        /// 缓存容量
        /// </summary>
        int CacheCapacity { get; set; }

        /// <summary>
        /// 判断当前通道能否调用<see cref="MoveNext()"/>
        /// </summary>
        bool CanMoveNext { get; }

        /// <summary>
        /// 能否写入
        /// </summary>
        bool CanWrite { get; }

        /// <summary>
        /// 通道Id
        /// </summary>
        int Id { get; }

        /// <summary>
        /// 最后一次操作时显示消息
        /// </summary>
        string LastOperationMes { get; }

        /// <summary>
        /// 元数据
        /// </summary>
        Metadata Metadata { get; }

        /// <summary>
        /// 状态
        /// </summary>
        ChannelStatus Status { get; }

        /// <summary>
        /// 目的Id地址。仅当该通道由两个客户端打通时有效。
        /// </summary>
        string TargetId { get; }

        /// <summary>
        /// 超时时间，默认1000*10ms。
        /// </summary>
        TimeSpan Timeout { get; set; }

        /// <summary>
        /// 是否被使用
        /// </summary>
        bool Using { get; }

        /// <summary>
        /// 取消
        /// </summary>
        void Cancel(string operationMes = null);

        /// <summary>
        /// 异步取消
        /// </summary>
        /// <returns></returns>
        Task CancelAsync(string operationMes = null);

        /// <summary>
        /// 完成操作
        /// </summary>
        void Complete(string operationMes = null);

        /// <summary>
        /// 异步完成操作
        /// </summary>
        /// <returns></returns>
        Task CompleteAsync(string operationMes = null);

        /// <summary>
        /// 获取当前的有效数据。在使用之后，请进行显式的<see cref="IDisposable.Dispose"/>调用。
        /// </summary>
        ByteBlock GetCurrent();

        /// <summary>
        /// 继续。
        /// <para>调用该指令时，接收方会跳出接收，但是通道依然可用，所以接收方需要重新调用<see cref="MoveNext()"/></para>
        /// </summary>
        /// <param name="operationMes"></param>
        void HoldOn(string operationMes = null);

        /// <summary>
        /// 异步调用继续
        /// </summary>
        /// <param name="operationMes"></param>
        /// <returns></returns>
        Task HoldOnAsync(string operationMes = null);

        /// <summary>
        /// 转向下个元素
        /// </summary>
        /// <returns></returns>
        bool MoveNext();

        /// <summary>
        /// 转向下个元素
        /// </summary>
        /// <returns></returns>
        Task<bool> MoveNextAsync();

        /// <summary>
        /// 写入通道
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        void Write(byte[] data, int offset, int length);

        /// <summary>
        /// 写入通道
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        Task WriteAsync(byte[] data, int offset, int length);
    }
}