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

            if (path == "/") path = "/website-cacher://static-content/controlPanel.html";

            var controlProtocol = "/website-cacher://";
            if (path.StartsWith(controlProtocol))
            {
                var controller = new ServerDriver(this);
                await controller.Process(path.Substring(controlProtocol.Length), context);
                controller.Output(context.Response);
            } else
            {
                var controller = new ResourceController(this);
                await controller.Process(path.Substring(1), context);
                controller.Output(context.Response);
            }

            context.Response.OutputStream.Close();
        }

        public void Stop()
        {
            this.Listener.Stop();
        }
    }
}
