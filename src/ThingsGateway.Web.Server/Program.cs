namespace ThingsGateway.Web.Entry
{
    /// <summary>
    /// ����
    /// </summary>
    public class Program
    {
        /// <summary>
        /// ��
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


            //��Ҫ�����ػ��ɰ�װ
            //builder.Host.UseWindowsService();
            //builder.Host.UseSystemd();

            builder.Inject();
            var app = builder.Build();
            app.Run();

        }
    }
}