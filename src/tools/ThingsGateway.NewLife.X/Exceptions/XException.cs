//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.ComponentModel;

namespace NewLife;

/// <summary>异常事件参数</summary>
public class ExceptionEventArgs : CancelEventArgs
{
    /// <summary>实例化</summary>
    /// <param name="action"></param>
    /// <param name="ex"></param>
    public ExceptionEventArgs(String action, Exception ex)
    {
        Action = action;
        Exception = ex;
    }

    /// <summary>发生异常时进行的动作</summary>
    public String Action { get; set; }

    /// <summary>异常</summary>
    public Exception Exception { get; set; }
}

/// <summary>X组件异常</summary>
[Serializable]
public class XException : Exception
{
    #region 构造

    /// <summary>初始化</summary>
    public XException()
    { }

    /// <summary>初始化</summary>
    /// <param name="message"></param>
    public XException(String message) : base(message) { }

    /// <summary>初始化</summary>
    /// <param name="format"></param>
    /// <param name="args"></param>
    public XException(String format, params Object?[] args) : base(String.Format(format, args)) { }

    /// <summary>初始化</summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public XException(String message, Exception innerException) : base(message, innerException) { }

    /// <summary>初始化</summary>
    /// <param name="innerException"></param>
    /// <param name="format"></param>
    /// <param name="args"></param>
    public XException(Exception innerException, String format, params Object?[] args) : base(String.Format(format, args), innerException) { }

    /// <summary>初始化</summary>
    /// <param name="innerException"></param>
    public XException(Exception innerException) : base((innerException?.Message), innerException) { }

    ///// <summary>初始化</summary>
    ///// <param name="info"></param>
    ///// <param name="context"></param>
    //protected XException(SerializationInfo info, StreamingContext context) : base(info, context) { }

    #endregion 构造
}

/// <summary>异常助手</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class ExceptionHelper
{
    /// <summary>是否对象已被释放异常</summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public static Boolean IsDisposed(this Exception ex) => ex is ObjectDisposedException;
}
