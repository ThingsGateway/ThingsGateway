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
namespace ThingsGateway.Web.Entry;

/// <summary>
/// 服务扩展
/// </summary>
public class SevicesExtension
{
    /// <summary>
    /// 读取组态王不能后台启动，所以这里多出来一个解决方案配置
    /// </summary>
    public static void KINGVIEWCONFIG()
    {
        #region Windows下 单进程启动,存在同名进程时直接退出
        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
        {
            if (WindowsControl.GetRunningInstance(typeof(Program).Assembly.FullName).Count > 0)
            {
                return;
            }
        }
        #endregion

        #region Windows下 窗口显示改变模式,隐藏
        //SW_HIDE：隐藏窗口并激活其他窗口。nCmdShow = 0。
        //SW_SHOWMINIMIZED：激活窗口并将其最小化。nCmdShow = 2。
        //SW_MAXIMIZE：最大化指定的窗口。nCmdShow = 3。
        //SW_SHOWMAXIMIZED：激活窗口并将其最大化。nCmdShow = 3。
        //SW_SHOWNOACTIVATE：以窗口最近一次的大小和状态显示窗口。激活窗口仍然维持激活状态。nCmdShow = 4。
        //SW_SHOW：在窗口原来的位置以原来的尺寸激活和显示窗口。nCmdShow = 5。
        //SW_MINIMIZE：最小化指定的窗口并且激活在Z序中的下一个顶层窗口。nCmdShow = 6。
        //SW_SHOWMINNOACTIVE：窗口最小化，激活窗口仍然维持激活状态。nCmdShow = 7。
        //SW_SHOWNA：以窗口原来的状态显示窗口。激活窗口仍然维持激活状态。nCmdShow = 8。
        //SW_RESTORE：激活并显示窗口。如果窗口最小化或最大化，则系统将窗口恢复到原来的尺寸和位置。在恢复最小化窗口时，应用程序应该指定这个标志。nCmdShow = 9。
        //SW_SHOWDEFAULT：依据在STARTUPINFO结构中指定的SW_FLAG标志设定显示状态，STARTUPINFO 结构是由启动应用程序的程序传递给CreateProcess函数的。nCmdShow = 10。
        //SW_SHOWNORMAL：激活并显示一个窗口。如果窗口被最小化或最大化，系统将其恢复到原来的尺寸和大小。应用程序在第一次显示窗口的时候应该指定此标志。nCmdShow = 1。
        //SW_FORCEMINIMIZE：在WindowNT5.0中最小化窗口，即使拥有窗口的线程被挂起也会最小化。在从其他线程最小化窗口时才使用这个参数。nCmdShow = 11。
        //窗口显示改变模式
        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
        {
            Console.Title = typeof(Program).Assembly.FullName;
            WindowsControl.HandleRunningInstance(Console.Title, 0);
        }

        #endregion

    }


}