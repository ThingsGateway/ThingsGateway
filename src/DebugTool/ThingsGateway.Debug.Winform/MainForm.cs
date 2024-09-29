// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

using Microsoft.AspNetCore.Components.WebView;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Web.WebView2.Core;

using System.Windows.Forms;

namespace ThingsGateway.Debug
{
    public partial class MainForm : Form
    {
        protected string UploadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "uploads");
        private BlazorWebView blazorWebView;


        public MainForm(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            //默认全屏
            //this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            //this.FormBorderStyle =FormBorderStyle.None;
            //this.TopMost = true;
            //this.KeyPreview = true;
            KeyUp += new System.Windows.Forms.KeyEventHandler(MainForm_KeyUp);

            blazorWebView = new BlazorWebView()
            {
                Dock = DockStyle.Fill,
                HostPage = "wwwroot/index.html",
                Services = serviceProvider
            };

            FormClosing += Program.Closing;

            blazorWebView.RootComponents.Add<Routes>("#app");
            Controls.Add(blazorWebView);
            blazorWebView.BringToFront();
            blazorWebView.KeyUp += new System.Windows.Forms.KeyEventHandler(MainForm_KeyUp);

            blazorWebView.BlazorWebViewInitialized += BlazorWebViewInitialized;

            blazorWebView.UrlLoading +=
                (sender, urlLoadingEventArgs) =>
                {
                    if (urlLoadingEventArgs.Url.Host != "0.0.0.0")
                    {
                        //外部链接WebView内打开,例如pdf浏览器
                        Console.WriteLine(urlLoadingEventArgs.Url);
                        urlLoadingEventArgs.UrlLoadingStrategy =
                            UrlLoadingStrategy.OpenInWebView;
                    }
                };
        }

        private void BlazorWebViewInitialized(object? sender, EventArgs e)
        {
            //下载开始时引发 DownloadStarting，阻止默认下载
            blazorWebView.WebView.CoreWebView2.DownloadStarting += CoreWebView2_DownloadStarting;

            //指定下载保存位置
            blazorWebView.WebView.CoreWebView2.Profile.DefaultDownloadFolderPath = UploadPath;

            ////[无依赖发布webview2程序] 固定版本运行时环境的方式来实现加载网页
            ////设置web用户文件夹 
            //var browserExecutableFolder = "c:\\wb2";
            //var userData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "BlazorWinFormsApp");
            //Directory.CreateDirectory(userData);
            //var creationProperties = new CoreWebView2CreationProperties()
            //{
            //    UserDataFolder = userData,
            //    BrowserExecutableFolder = browserExecutableFolder
            //};
            //mainBlazorWebView.WebView.CreationProperties = creationProperties;
        }

        private void CoreWebView2_DownloadStarting(object? sender, CoreWebView2DownloadStartingEventArgs e)
        {
            var downloadOperation = e.DownloadOperation;
            string fileName = Path.GetFileName(e.ResultFilePath);
            var filePath = Path.Combine(UploadPath, fileName);

            //指定下载保存位置
            e.ResultFilePath = filePath;
            MessageBox.Show($"下载文件完成 {fileName}", "提示");
        }

        private void MainForm_KeyUp(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                if (WindowState == System.Windows.Forms.FormWindowState.Normal)
                {
                    WindowState = System.Windows.Forms.FormWindowState.Maximized;
                    FormBorderStyle = FormBorderStyle.None;
                }
                else
                {
                    WindowState = System.Windows.Forms.FormWindowState.Normal;
                    FormBorderStyle = FormBorderStyle.Sizable;
                }
            }
        }


    }
}
