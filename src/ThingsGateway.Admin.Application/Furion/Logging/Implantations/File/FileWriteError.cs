
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------




// 版权归百小僧及百签科技（广东）有限公司所有。

namespace ThingsGateway.Logging;

/// <summary>
/// 文件写入错误信息上下文
/// </summary>
public sealed class FileWriteError
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="currentFileName">当前日志文件名</param>
    /// <param name="exception">异常对象</param>
    internal FileWriteError(string currentFileName, Exception exception)
    {
        CurrentFileName = currentFileName;
        Exception = exception;
    }

    /// <summary>
    /// 当前日志文件名
    /// </summary>
    public string CurrentFileName { get; private set; }

    /// <summary>
    /// 引起文件写入异常信息
    /// </summary>
    public Exception Exception { get; private set; }

    /// <summary>
    /// 备用日志文件名
    /// </summary>
    internal string RollbackFileName { get; private set; }

    /// <summary>
    /// 配置日志文件写入错误后新的备用日志文件名
    /// </summary>
    /// <param name="rollbackFileName">备用日志文件名</param>
    public void UseRollbackFileName(string rollbackFileName)
    {
        RollbackFileName = rollbackFileName;
    }
}