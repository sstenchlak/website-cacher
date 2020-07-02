using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebsiteCacher.ServerControllers
{
    /// <summary>
    /// Default controller as driver to all function through http protocol
    /// </summary>
    class ServerDriver : AbstractServerController
    {
        private AbstractServerController Controller = null;

        public override void Output(HttpListenerResponse output)
        {
            if (Controller == null) throw new InvalidOperationException("There is no controller to call Output on.");

            Controller.Output(output);
        }

        public override async Task Process(string parameter, HttpListenerContext context)
        {
            var parts = parameter.Split('/', 2);

            switch (parts[0])
            {
                case "resource-status":
                    Controller = new ResourceStatusController(this.ServerContext);
                    break;
                case "static-content":
                    Controller = new StaticFileController(this.ServerContext);
                    break;
            }

            await Controller.Process(parts.Length < 2 ? "" : parts[1], context);
        }

        public ServerDriver(Server serverContext) : base(serverContext) { }
    }
}
