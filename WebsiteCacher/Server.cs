using System;
using System.Net;
using System.Threading.Tasks;
using WebsiteCacher.ServerControllers;

namespace WebsiteCacher
{
    class Server
    {
        private readonly HttpListener Listener = new HttpListener();
        public readonly ResourceManager ResourceManager;
        public readonly PageQueryManager PageQueryManager;

        public Server(int port, ResourceManager resourceManager, PageQueryManager pageQueryManager)
        {
            this.Listener.Prefixes.Add($"http://localhost:{port}/");
            this.ResourceManager = resourceManager;
            this.PageQueryManager = pageQueryManager;
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

            var controlProtocol = "/website-cacher://";
            if (path.StartsWith(controlProtocol))
            {
                var controller = new ServerDriver(this);
                await controller.Process(path.Substring(controlProtocol.Length), context);
                controller.Output(context.Response);
            }
            else if (path == "/")
            {
                var controller = new ServerDriver(this);
                await controller.Process("static-content/controlPanel.html", context);
                controller.Output(context.Response);
            } else
            {
                await this.ActionResource(path.Substring(1), context.Response);
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
