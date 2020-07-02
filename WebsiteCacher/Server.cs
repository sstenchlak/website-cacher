using System;
using System.Net;
using System.Threading.Tasks;

namespace WebsiteCacher
{
    class Server
    {
        private readonly HttpListener Listener = new HttpListener();
        private readonly ResourceManager ResourceManager;

        public Server(int port, ResourceManager resourceManager)
        {
            this.Listener.Prefixes.Add($"http://localhost:{port}/");
            this.ResourceManager = resourceManager;
        }

        public async Task Start()
        {
            this.Listener.Start();
            while (true)
            {
                var context = await this.Listener.GetContextAsync();
                this.OnContext(context);
            }
        }

        private async void OnContext(HttpListenerContext context)
        {
            var path = context.Request.Url.PathAndQuery;
            // First directory in url
            var action = context.Request.Url.Segments.Length >= 1 ? context.Request.Url.Segments[1] : null;


            var start = context.Request.Url.PathAndQuery.IndexOf('/', 1);
            string query = "";
            if (start > 0)
            {
                query = context.Request.Url.PathAndQuery.Substring(start + 1);
            }

            // Simple router
            switch (action)
            {
                case "r/":
                    await this.ActionResource(query, context.Response);
                    break;
                default:
                    context.Response.StatusCode = 400;
                    break;

            }

            context.Response.OutputStream.Close();
        }

        /// <summary>
        /// Handles URL when youser want a resource
        /// </summary>
        /// <param name="query"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        private async Task ActionResource(string query, HttpListenerResponse response)
        {
            Console.WriteLine(query);
            var resource = this.ResourceManager.GetResource(query);

            if (resource != null && resource.IsDownloaded)
            {
                if (resource.ContentType != null && resource.ContentType.StartsWith("text/html"))
                {
                    var pm = new PageModifier(resource.Get(), query);
                    pm.Process();
                    await pm.GetResult().CopyToAsync(response.OutputStream);
                } else
                {
                    await resource.Get().CopyToAsync(response.OutputStream);
                }

                response.ContentType = resource.ContentType;
            } else
            {
                response.StatusCode = 404;
            }
        }

        public void Stop()
        {
            this.Listener.Stop();
        }
    }
}
