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

//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation;
/// <summary>
/// 快捷日志
/// </summary>
public class TGEasyLogger : LoggerBase
{
    private readonly Action<LogType, object, string, Exception> m_action;
    private readonly Action<string> m_action1;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="action">参数依次为：日志类型，触发源，消息，异常</param>
    public TGEasyLogger(Action<LogType, object, string, Exception> action)
    {
        m_action = action;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="action">参数为日志消息输出。</param>
    public TGEasyLogger(Action<string> action)
    {
        m_action1 = action;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="logType"></param>
    /// <param name="source"></param>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    protected override void WriteLog(LogType logType, object source, string message, Exception exception)
    {
        try
        {
            if (m_action != null)
            {
                m_action.Invoke(logType, source, message, exception);
                return;
            }
            if (m_action1 != null)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fffffff zz"));
                stringBuilder.Append(" | ");
                stringBuilder.Append(logType.ToString());
                stringBuilder.Append(" | ");
                stringBuilder.Append(message);

                if (exception != null)
                {
                    stringBuilder.Append(" | ");
                    stringBuilder.Append($"【异常消息】：{exception.Message}");
                    stringBuilder.Append($"【堆栈】：{(exception == null ? "未知" : exception.StackTrace)}");
                }
                stringBuilder.AppendLine();
                m_action1.Invoke(stringBuilder.ToString());
                return;
            }
        }
        catch
        {
        }
    }
}