using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace WebsiteCacher.ServerControllers
{
    /// <summary>
    /// This controller gives information about downloaded resources.
    /// Expected format is website-cacher://resource-status/<resourceURL>
    /// </summary>
    class ResourceStatusController : AbstractServerController
    {
        abstract class OutputResult
        {
            public string status;
            public string url;
        }

        class NoResult : OutputResult { }
        class PositiveResult : OutputResult
        {
            public string contentType;
            public DateTime time;
        }

        private OutputResult Result = null;

        public override void Output(HttpListenerResponse output)
        {
            var data = JsonConvert.SerializeObject(Result);
            using var writer = new StreamWriter(output.OutputStream);
            writer.Write(data);
        }

        public override Task Process(string parameter, HttpListenerContext context)
        {
            var resource = this.ServerContext.ResourceManager.GetResource(parameter);

            if (resource == null)
            {
                Result = new NoResult{ status = "not-found" };
            } else
            {
                if (!resource.IsDownloaded)
                {
                    Result = new NoResult { status = "not-downloaded" };
                } else
                {
                    Result = new PositiveResult
                    {
                        status = "downloaded",
                        contentType = resource.ContentType,
                        time = resource.Updated,
                    };
                }
            }

            Result.url = parameter;

            return Task.CompletedTask;
        }

        public ResourceStatusController(Server serverContext) : base(serverContext) { }
    }
}
