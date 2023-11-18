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

using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;

using System.Diagnostics;
using System.Threading.Tasks;

using ThingsGateway.Components;

namespace ThingsGateway.Foundation.Demo.Winform
{
    public partial class MainFrom : Form
    {
        public MainFrom()
        {
            InitializeComponent();

            IServiceCollection services = null;

            Serve.RunNative(a =>
            {
                services = a;
                services.AddWindowsFormsBlazorWebView();
                services.ThingsGatewayComponentsConfigureServices();
            });

            blazorWebView1.HostPage = "wwwroot/index.html";
            blazorWebView1.Services = services.BuildServiceProvider();
            this.Text = "ThingsGateway.Foundation.Demo";
            blazorWebView1.RootComponents.Add<ThingsGateway.Foundation.Demo.App>("#app");
        }

        private void MainFrom_FormClosed(object sender, FormClosedEventArgs e)
        {

            Task.Run(() => { MessageBox.Show("释放资源中，稍候自动退出程序..."); });
            Task.Run(async () =>
            {
                await Task.Delay(3000);
                Process.GetCurrentProcess().Kill();
            });

        }
    }
}