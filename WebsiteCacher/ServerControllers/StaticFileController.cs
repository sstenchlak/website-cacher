using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebsiteCacher.ServerControllers
{
    /// <summary>
    /// This controller can open and serve local files from static folder.
    /// Used for injecting custom .js file to each document.
    /// </summary>
    class StaticFileController : AbstractServerController
    {
        private string Path;

        public override void Output(HttpListenerResponse output)
        {
            var location = "./static/" + Path;
            if (File.Exists(location))
            {
                using var file = new FileStream(location, FileMode.Open, FileAccess.Read);
                file.CopyTo(output.OutputStream);
            } else
            {
                output.StatusCode = 404;
            }
        }

        public override Task Process(string parameter, HttpListenerContext context)
        {
            Path = parameter;

            return Task.CompletedTask;
        }

        public StaticFileController(Server serverContext) : base(serverContext) { }
    }
}
