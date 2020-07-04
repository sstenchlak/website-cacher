using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace WebsiteCacher.ServerControllers
{
    /// <summary>
    /// This controller gives information about downloaded resources.
    /// Expected format is website-cacher://resource/status/<resourceURL>
    ///                    website-cacher://resource/garbage-collect
    /// </summary>
    class ResourceController : AbstractServerController
    {
        abstract class AStatusResult
        {
            public string status;
            public string url;
        }

        class NoResult : AStatusResult { }
        class PositiveResult : AStatusResult
        {
            public string contentType;
            public DateTime time;
        }

        /// <summary>
        /// Interface for garbage-collect request
        /// </summary>
        class GCResult
        {
            public int removed;
            public int kept;
        }

        private AStatusResult ResultStatus = null;
        private GCResult ResultGC = null;


        /// <summary>
        /// Parameter used by the client (status or garbage-collect)
        /// </summary>
        private string Parameter = null;

        public override void Output(HttpListenerResponse output)
        {
            string data = "";

            if (Parameter == "status")
            {
                data = JsonConvert.SerializeObject(ResultStatus);
            }

            if (Parameter == "garbage-collect")
            {
                data = JsonConvert.SerializeObject(ResultGC);
            }

            using var writer = new StreamWriter(output.OutputStream);
            writer.Write(data);
        }

        public override Task Process(string parameter, HttpListenerContext context)
        {
            if (parameter.StartsWith("status/"))
            {
                Parameter = "status";
                var url = parameter.Substring("status/".Length);
                var resource = this.ServerContext.ResourceManager.GetResource(url);

                if (resource == null)
                {
                    ResultStatus = new NoResult { status = "not-found" };
                }
                else
                {
                    if (!resource.IsDownloaded)
                    {
                        ResultStatus = new NoResult { status = "not-downloaded" };
                    }
                    else
                    {
                        ResultStatus = new PositiveResult
                        {
                            status = "downloaded",
                            contentType = resource.ContentType,
                            time = resource.Updated,
                        };
                    }
                }

                ResultStatus.url = url;
            }

            if (parameter == "garbage-collect")
            {
                Parameter = parameter;
                ResultGC = new GCResult();
                ServerContext.ResourceManager.ResourceGC(out ResultGC.removed, out ResultGC.kept);
            }

            return Task.CompletedTask;
        }

        public ResourceController(Server serverContext) : base(serverContext) { }
    }
}
