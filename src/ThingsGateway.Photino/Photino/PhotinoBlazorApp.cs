using Microsoft.Extensions.DependencyInjection;

using Photino.NET;

namespace Photino.Blazor
{
    public class PhotinoBlazorApp
    {
        /// <summary>
        /// Gets configuration for the service provider.
        /// </summary>
        public IServiceProvider Services { get; private set; }

        /// <summary>
        /// Gets configuration for the root components in the window.
        /// </summary>
        public BlazorWindowRootComponents RootComponents { get; private set; }

        internal void Initialize(IServiceProvider services, RootComponentList rootComponents)
        {
            Services = services;
            RootComponents = Services.GetService<BlazorWindowRootComponents>();
            MainWindow = Services.GetService<PhotinoWindow>();
            WindowManager = Services.GetService<PhotinoWebViewManager>();

            MainWindow
                .SetTitle("Photino.Blazor App")
                .SetUseOsDefaultSize(false)
                .SetUseOsDefaultLocation(false)
                .SetWidth(1000)
                .SetHeight(900)
                .SetLeft(450)
                .SetTop(100);

            MainWindow.RegisterCustomSchemeHandler(PhotinoWebViewManager.BlazorAppScheme, HandleWebRequest);

            foreach (var component in rootComponents)
            {
                RootComponents.Add(component.Item1, component.Item2);
            }
        }

        public PhotinoWindow MainWindow { get; private set; }

        public PhotinoWebViewManager WindowManager { get; private set; }

        public void Run()
        {
            if (string.IsNullOrWhiteSpace(MainWindow.StartUrl))
                MainWindow.StartUrl = "/";

            WindowManager.Navigate(MainWindow.StartUrl);
            MainWindow.WaitForClose();
        }

        public Stream HandleWebRequest(object sender, string scheme, string url, out string contentType)
                => WindowManager.HandleWebRequest(sender, scheme, url, out contentType!)!;
    }
}
