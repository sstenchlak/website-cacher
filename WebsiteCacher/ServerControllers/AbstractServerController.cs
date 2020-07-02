using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebsiteCacher.ServerControllers
{
    /// <summary>
    /// Common ancestor for all controllers responsible for manipulating server data and returning them.
    /// It follows very simple MVC architecture.
    /// </summary>
    abstract class AbstractServerController
    {
        protected readonly Server ServerContext;

        /// <summary>
        /// This function process the request and makes changes in the db if necessary.
        /// </summary>
        /// <param name="parameter">Rest of url address usable for this controller</param>
        /// <param name="context">Full HttpListenerContext if necessary</param>
        /// <returns></returns>
        public abstract Task Process(string parameter, HttpListenerContext context);

        /// <summary>
        /// Outputs its result into output stream. This method is separated from <see cref="Process(string, HttpListenerContext)"/>
        /// for more complex scenarios when multiple controllers are used together.
        /// </summary>
        /// <param name="output"></param>
        /// <returns></returns>
        public abstract void Output(HttpListenerResponse output);

        public AbstractServerController(Server serverContext)
        {
            ServerContext = serverContext;
        }
    }
}
