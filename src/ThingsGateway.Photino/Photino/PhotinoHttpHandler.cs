using System.Net;
using System.Net.Http.Headers;

namespace Photino.Blazor
{
    public class PhotinoHttpHandler : DelegatingHandler
    {
        private readonly PhotinoBlazorApp app;

        //use this constructor if a handler is registered in DI to inject dependencies
        public PhotinoHttpHandler(PhotinoBlazorApp app) : this(app, null)
        {
        }

        //Use this constructor if a handler is created manually.
        //Otherwise, use DelegatingHandler.InnerHandler public property to set the next handler.
        public PhotinoHttpHandler(PhotinoBlazorApp app, HttpMessageHandler innerHandler)
        {
            this.app = app;

            //the last (inner) handler in the pipeline should be a "real" handler.
            //To make a HTTP request, create a HttpClientHandler instance.
            InnerHandler = innerHandler ?? new HttpClientHandler();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Stream content = app.HandleWebRequest(null, null, request.RequestUri.AbsoluteUri, out string contentType);
            if (content != null)
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StreamContent(content);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                return response;
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
