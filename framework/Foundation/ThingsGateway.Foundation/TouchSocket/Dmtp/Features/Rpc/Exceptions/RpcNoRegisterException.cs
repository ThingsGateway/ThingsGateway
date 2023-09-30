﻿#region copyright
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

namespace ThingsGateway.Foundation.Dmtp.Rpc
{
    /// <summary>
    /// Rpc无注册异常
    /// </summary>
    [Serializable]
    public class RpcNoRegisterException : Exception
    {
        /// <summary>
        ///构造函数
        /// </summary>
        public RpcNoRegisterException() : base() { }

        /// <summary>
        ///构造函数
        /// </summary>
        /// <param name="message"></param>
        public RpcNoRegisterException(string message) : base(message) { }

        /// <summary>
        ///构造函数
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public RpcNoRegisterException(string message, System.Exception inner) : base(message, inner) { }

        /// <summary>
        ///构造函数
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected RpcNoRegisterException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}