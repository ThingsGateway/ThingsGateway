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
            builder.Host.ConfigureWindowsService();
            builder.Host.ConfigureLinuxService();

            builder.Inject();
            var app = builder.Build();
            app.Run();

        }
    }
}