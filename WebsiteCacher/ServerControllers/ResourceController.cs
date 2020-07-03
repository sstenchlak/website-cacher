using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace WebsiteCacher.ServerControllers
{
    /// <summary>
    /// This controller catches all requests for accessing resource from cache
    /// </summary>
    class ResourceController : AbstractServerController
    {
        public override void Output(HttpListenerResponse output)
        {
            output.StatusCode = StatusCode;
            output.ContentType = ContentType;
            ResultStream.CopyTo(output.OutputStream);
        }

        private Stream ResultStream = Stream.Null;
        private string ContentType = "";
        private int StatusCode = 200;

        public override Task Process(string parameter, HttpListenerContext context)
        {
            var resource = ServerContext.ResourceManager.GetResource(parameter);

            if (resource != null && resource.IsDownloaded)
            {
                if (resource.ContentType != null && resource.ContentType.StartsWith("text/html"))
                {
                    var pm = new PageModifier(resource.Get(), parameter);
                    pm.Process();
                    ResultStream = pm.GetResult();
                }
                else
                {
                    ResultStream = resource.Get();
                }

                ContentType = resource.ContentType;
            }
            else
            {
                StatusCode = 404;
            }

            return Task.CompletedTask;
        }

        public ResourceController(Server serverContext) : base(serverContext) { }
    }
}
