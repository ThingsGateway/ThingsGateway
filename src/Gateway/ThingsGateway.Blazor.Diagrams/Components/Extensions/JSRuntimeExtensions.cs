using Microsoft.JSInterop;

using ThingsGateway.Blazor.Diagrams.Core.Geometry;

namespace ThingsGateway.Blazor.Diagrams.Extensions;

public static class JSRuntimeExtensions
{
    public static async Task<Rectangle> GetBoundingClientRect(this IJSRuntime jsRuntime, ElementReference element)
    {
        return await jsRuntime.InvokeAsync<Rectangle>("BlazorDiagrams.getBoundingClientRect", element).ConfigureAwait(false);
    }

    public static async Task ObserveResizes<T>(this IJSRuntime jsRuntime, ElementReference element,
        DotNetObjectReference<T> reference) where T : class
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("BlazorDiagrams.observe", element, reference, element.Id).ConfigureAwait(false);
        }
        catch (ObjectDisposedException)
        {
            // Ignore, DotNetObjectReference was likely disposed
        }
    }

    public static async Task UnobserveResizes(this IJSRuntime jsRuntime, ElementReference element)
    {
        await jsRuntime.InvokeVoidAsync("BlazorDiagrams.unobserve", element, element.Id).ConfigureAwait(false);
    }
}
