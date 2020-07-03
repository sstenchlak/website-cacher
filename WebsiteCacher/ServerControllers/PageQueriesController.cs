using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace WebsiteCacher.ServerControllers
{
    /// <summary>
    /// This controller handles all the operation with PageQuery, such as listing and adding new ones.
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

            using var writer = new StreamWriter(output.OutputStream);
            writer.Write(data);
        }

        public override async Task Process(string parameter, HttpListenerContext context)
        {
            this.Parameter = parameter;

            if (parameter == "list")
            {
                ResultList = new List<PageQueryInterface>();

                foreach (var query in ServerContext.PageQueryManager.GetAll())
                {
                    ResultList.Add((PageQueryInterface)query);
                }
            }

            if (parameter == "add")
            {
                var request = context.Request;
                string text;
                using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
                text = reader.ReadToEnd();

                PageQueryInterface result = JsonConvert.DeserializeObject<PageQueryInterface>(text);

                CreatedQuery = await ServerContext.PageQueryManager.CreateNew();
                CreatedQuery.Depth = result.depth;
                CreatedQuery.MediaRegex = result.media_regexp;
                CreatedQuery.PageRegex = result.page_regexp;
                CreatedQuery.ToleratedAge = result.time;
                CreatedQuery.StartingURL = result.url;

                await CreatedQuery.Scrape();
            }
        }
    }
}
