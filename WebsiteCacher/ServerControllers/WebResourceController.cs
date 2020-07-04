using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace WebsiteCacher.ServerControllers
{
    /// <summary>
    /// Controller serving cached resources.
    /// </summary>
    class WebResourceController : AbstractServerController
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
            var modifiedUrl = HtmlProcessor.SimplifyUrl(parameter);
            var resource = ServerContext.ResourceManager.GetResource(modifiedUrl);

            if (resource != null && resource.IsDownloaded)
            {
                if (resource.ContentType != null && resource.ContentType.StartsWith("text/html"))
                {
                    using var stream = resource.Get();
                    var pm = new PageModifier(stream, modifiedUrl);
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

        public WebResourceController(Server serverContext) : base(serverContext) { }
    }
}
