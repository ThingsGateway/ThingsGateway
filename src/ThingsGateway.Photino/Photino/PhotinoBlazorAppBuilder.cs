using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

using System.Collections;

namespace Photino.Blazor
{
    public class PhotinoBlazorAppBuilder
    {
        internal PhotinoBlazorAppBuilder()
        {
            RootComponents = new RootComponentList();
            Services = new ServiceCollection();
        }

        public static PhotinoBlazorAppBuilder CreateDefault(string[] args = default)
        {
            return CreateDefault(null, args);
        }

        public static PhotinoBlazorAppBuilder CreateDefault(IFileProvider fileProvider, string[] args = default)
        {
            // We don't use the args for anything right now, but we want to accept them
            // here so that it shows up this way in the project templates.
            // var jsRuntime = DefaultWebAssemblyJSRuntime.Instance;
            var builder = new PhotinoBlazorAppBuilder();
            builder.Services.AddBlazorDesktop(fileProvider);

            // Right now we don't have conventions or behaviors that are specific to this method
            // however, making this the default for the template allows us to add things like that
            // in the future, while giving `new BlazorDesktopHostBuilder` as an opt-out of opinionated
            // settings.
            return builder;
        }

        public RootComponentList RootComponents { get; set; }

        public IServiceCollection Services { get; set; }
        public PhotinoBlazorApp Build(Action<IServiceProvider> serviceProviderOptions = null)
        {
            // register root components with DI container
            // Services.AddSingleton(RootComponents);

            var sp = Services.BuildServiceProvider();
            var app = sp.GetRequiredService<PhotinoBlazorApp>();

            serviceProviderOptions?.Invoke(sp);

            app.Initialize(sp, RootComponents);
            return app;
        }
        public PhotinoBlazorApp Build(IServiceProvider serviceProvider = null)
        {
            // register root components with DI container
            // Services.AddSingleton(RootComponents);

            var sp = serviceProvider ?? Services.BuildServiceProvider();
            var app = sp.GetRequiredService<PhotinoBlazorApp>();

            app.Initialize(sp, RootComponents);
            return app;
        }
    }

    public class RootComponentList : IEnumerable<(Type, string)>
    {
        private readonly List<(Type componentType, string domElementSelector)> components = new List<(Type componentType, string domElementSelector)>();

        public void Add<TComponent>(string selector) where TComponent : IComponent
        {
            components.Add((typeof(TComponent), selector));
        }

        public void Add(Type componentType, string selector)
        {
            if (!typeof(IComponent).IsAssignableFrom(componentType))
            {
                throw new ArgumentException("The component type must implement IComponent interface.");
            }

            components.Add((componentType, selector));
        }

        public IEnumerator<(Type, string)> GetEnumerator()
        {
            return components.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return components.GetEnumerator();
        }
    }
}
