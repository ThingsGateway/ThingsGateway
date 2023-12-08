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

namespace ThingsGateway.Foundation.Adapter.DLT645
{
    /// <summary>
    /// DLT645_2007
    /// </summary>
    public interface IDLT645_2007 : IReadWrite
    {
        /// <summary>
        /// 增加FE FE FE FE的报文头部
        /// </summary>
        bool EnableFEHead { get; set; }

        /// <summary>
        /// 操作员代码
        /// </summary>
        string OperCode { get; set; }

        /// <summary>
        /// 写入密码
        /// </summary>
        string Password { get; set; }

        /// <summary>
        /// 通讯地址BCD码，一般应该是12个字符
        /// </summary>
        string Station { get; set; }
    }
}