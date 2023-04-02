namespace ThingsGateway.Web.Entry
{
    /// <summary>
    /// 启动
    /// </summary>
    public class Program
    {
        /// <summary>
        /// 主
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                ContentRootPath = AppContext.BaseDirectory,
                WebRootPath = "wwwroot",
                Args = args
            });
            builder.WebHost.UseStaticWebAssets();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            builder.Host.UseContentRoot(AppContext.BaseDirectory);


            //需要服务守护可安装
            //builder.Host.UseWindowsService();
            //builder.Host.UseSystemd();

            builder.Inject();
            var app = builder.Build();
            app.Run();

        }
    }
}