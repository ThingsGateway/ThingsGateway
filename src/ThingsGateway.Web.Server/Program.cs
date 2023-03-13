namespace ThingsGateway.Web.Entry
{
    public class Program
    {
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

            /*
	<ItemGroup>
	  <PackageReference Include="Microsoft.Extensions.Hosting.Systemd" Version="7.0.0" />
	  <PackageReference Include="Microsoft.Extensions.Hosting.WindowsServices" Version="7.0.0" />
	</ItemGroup>
             * */
            //需要服务守护可安装
            //builder.Host.UseWindowsService();
            //builder.Host.UseSystemd();

            builder.Inject();
            var app = builder.Build();
            app.Run();

        }
    }
}