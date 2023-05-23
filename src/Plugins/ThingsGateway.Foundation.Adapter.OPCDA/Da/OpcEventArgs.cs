#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

namespace OpcDaClient.Da
{
    public delegate void OnDataChangedHandler(ItemReadResult[] opcItems);
    public delegate void OnWriteCompletedHandler(ItemWriteResult[] opcItems);
    public delegate void OnReadCompletedHandler(ItemReadResult[] opcItems);
    public class ItemReadResult
    {
        public string Name { get; set; } = "";
        public object Value { get; set; } = 0;
        public DateTime TimeStamp { get; set; }
        public short Quality { get; set; }
    }
    public class ItemWriteResult
    {
        public string Name { get; set; } = "";
        public int Exception { get; set; } = 0;
    }

    public class OpcEventArgs : EventArgs
    {
        /// <summary>
        /// group handel
        /// </summary>
        public int GroupHandle { get; set; }
        /// <summary>
        /// item lenght
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// item value
        /// </summary>
        public object[] Values { get; set; }
        /// <summary>
        /// error info
        /// </summary>
        public int[] Errors { get; set; }
        /// <summary>
        /// items handel
        /// </summary>
        public int[] ClientItemsHandle { get; set; }
    }
}
