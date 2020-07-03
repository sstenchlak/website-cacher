using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebsiteCacher.ServerControllers
{
    /// <summary>
    /// Handles requests for control section only when other controllers fails.
    /// </summary>
    class ErrorController : AbstractServerController
    {
        public override void Output(HttpListenerResponse output)
        {
            output.StatusCode = 404;
        }

        public override Task Process(string parameter, HttpListenerContext context)
        {
            return Task.CompletedTask;
        }

        public ErrorController(Server serverContext) : base(serverContext) { }
    }
}
