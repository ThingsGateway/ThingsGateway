using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

using Photino.NET;

namespace Photino.Blazor
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBlazorDesktop(this IServiceCollection services, IFileProvider fileProvider = null)
        {
            services
                .AddOptions<PhotinoBlazorAppConfiguration>()
                .Configure(opts =>
                {
                    opts.AppBaseUri = new Uri(PhotinoWebViewManager.AppBaseUri);
                    opts.HostPage = "index.html";
                });

            return services
                .AddScoped(sp =>
                {
                    var handler = sp.GetService<PhotinoHttpHandler>();
                    return new HttpClient(handler) { BaseAddress = new Uri(PhotinoWebViewManager.AppBaseUri) };
                })
                .AddSingleton(sp =>
                {
                    var manager = sp.GetService<PhotinoWebViewManager>();
                    var store = sp.GetService<JSComponentConfigurationStore>();

                    return new BlazorWindowRootComponents(manager, store);
                })
                .AddSingleton<Dispatcher, PhotinoDispatcher>()
                .AddSingleton<IFileProvider>(_ =>
                {
                    if (fileProvider is null)
                    {
                        var root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot");
                        return new PhysicalFileProvider(root);
                    }
                    else
                    {
                        return fileProvider;
                    }
                })
                .AddSingleton<JSComponentConfigurationStore>()
                .AddSingleton<PhotinoBlazorApp>()
                .AddSingleton<PhotinoHttpHandler>()
                .AddSingleton<PhotinoSynchronizationContext>()
                .AddSingleton<PhotinoWebViewManager>()
                .AddSingleton(new PhotinoWindow())
                .AddBlazorWebView();
        }
    }
}
