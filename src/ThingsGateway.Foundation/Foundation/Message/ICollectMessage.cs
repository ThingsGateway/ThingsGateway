namespace ThingsGateway.Foundation
{
    public interface IMessage : IOperResult, IRequestInfo
    {
        /// <summary>
        /// 实体数据长度
        /// </summary>
        int BodyLength { get; set; }

        /// <summary>
        /// 解析后的字节数据
        /// </summary>
        byte[] Content { get; set; }

        /// <summary>
        /// 消息头字节
        /// </summary>
        byte[] HeadBytes { get; }

        /// <summary>
        /// 消息头的指令长度
        /// </summary>
        int HeadBytesLength { get; }


        /// <summary>
        /// 接收的字节信息
        /// </summary>
        byte[] ReceivedBytes { get; set; }

        /// <summary>
        /// 发送的字节信息
        /// </summary>
        byte[] SendBytes { get; set; }

        /// <summary>
        /// 检查头子节的合法性,并赋值<see cref="BodyLength"/><br />
        /// </summary>
        /// <param name="head">接收的头子节</param>
        /// <returns>是否成功的结果</returns>
        bool CheckHeadBytes(byte[] head);
    }
}