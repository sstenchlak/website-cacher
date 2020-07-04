using Microsoft.Threading;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;
using SQLitePCL;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace WebsiteCacher.ServerControllers
{
    /// <summary>
    /// This controller handles all the operation with PageQuery, such as listing and adding new ones.
    /// 
    /// Curent scenations: website-cacher://page-queries/add - Add new PageQuery
    ///                    website-cacher://page-queries/list - Get all PageQueries
    ///                    website-cacher://page-queries/scrape/<id>
    ///                    website-cacher://page-queries/remove/<id>
    /// </summary>
    class PageQueriesController : AbstractServerController
    {
        /// <summary>
        /// Represents interface of PageQuery between server and client.
        /// </summary>
        class PageQueryInterface
        {
            public int id;
            public string url;
            public int depth;
            public int time;
            public string page_regexp;
            public string media_regexp;

            /// <summary>
            /// Explicit cast operator from real PageQuery
            /// </summary>
            /// <param name="query"></param>
            public static explicit operator PageQueryInterface(PageQuery query) => new PageQueryInterface {
                id = query.Id,
                url = query.StartingURL,
                depth = query.Depth,
                time = query.ToleratedAge,
                media_regexp = query.MediaRegex,
                page_regexp = query.PageRegex,
            };
        }

        /// <summary>
        /// Data for <code><see cref="Parameter"/> == "list"</code>
        /// </summary>
        private List<PageQueryInterface> ResultList;

        /// <summary>
        /// Data for <code><see cref="Parameter"/> == "add"</code>
        /// </summary>
        private PageQuery CreatedQuery;

        /// <summary>
        /// Data for <code><see cref="Parameter"/> == "scrape"</code>
        /// </summary>
        private bool OperationSuccessfull = false;

        /// <summary>
        /// Parameter used by the client (add, or list)
        /// </summary>
        private string Parameter = null;

        public PageQueriesController(Server serverContext) : base(serverContext) { }

        public override void Output(HttpListenerResponse output)
        {
            string data = null;

            if (Parameter == "list")
            {
                data = JsonConvert.SerializeObject(ResultList);
            }

            if (Parameter == "add")
            {
                data = JsonConvert.SerializeObject((PageQueryInterface)CreatedQuery);
            }

            if (Parameter.StartsWith("scrape/") || Parameter.StartsWith("remove/"))
            { 
                data = JsonConvert.SerializeObject(OperationSuccessfull);
            }

            using var writer = new StreamWriter(output.OutputStream);
            writer.Write(data);
        }

        public override async Task Process(string parameter, HttpListenerContext context)
        {
            this.Parameter = parameter;

            // Parse json request
            var request = context.Request;
            string text;
            using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
            text = reader.ReadToEnd();

            // GET
            if (parameter == "list")
            {
                ResultList = new List<PageQueryInterface>();

                foreach (var query in ServerContext.PageQueryManager.GetAll())
                {
                    ResultList.Add((PageQueryInterface)query);
                }
            }

            // POST
            if (parameter == "add")
            {
                PageQueryInterface result = JsonConvert.DeserializeObject<PageQueryInterface>(text);

                CreatedQuery = await ServerContext.PageQueryManager.CreateNew();
                CreatedQuery.Depth = result.depth;
                CreatedQuery.MediaRegex = result.media_regexp;
                CreatedQuery.PageRegex = result.page_regexp;
                CreatedQuery.ToleratedAge = result.time;
                CreatedQuery.StartingURL = result.url;
            }

            // POST
            if (parameter.StartsWith("scrape/"))
            {
                if (int.TryParse(parameter.Substring("scrape/".Length), out var id))
                {
                    var pageQuery = ServerContext.PageQueryManager.GetById(id);
                    if (pageQuery != null)
                    {
                        OperationSuccessfull = true;

                        _ = ScrapePageQueryAsync(pageQuery);
                    }
                }
            }

            // DELETE
            if (parameter.StartsWith("remove/"))
            {
                if (int.TryParse(parameter.Substring("remove/".Length), out var id))
                {
                    var pageQuery = ServerContext.PageQueryManager.GetById(id);
                    if (pageQuery != null)
                    {
                        OperationSuccessfull = true;

                        pageQuery.Remove();
                    }
                }
            }
        }

        private async Task ScrapePageQueryAsync(PageQuery pageQuery)
        {
            await Task.CompletedTask;
            await pageQuery.Scrape();
        }
    }
}
