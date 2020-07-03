using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebsiteCacher.ServerControllers
{
    class PageQueriesController : AbstractServerController
    {

        class PageQueryResult
        {
            public int id;
            public string url;
            public int depth;
            public int time;
            public string page_regexp;
            public string media_regexp;
        }

        List<PageQueryResult> ResultList;

        private string Parameter = null;

        public PageQueriesController(Server serverContext) : base(serverContext) { }

        public override void Output(HttpListenerResponse output)
        {
            string data = null;

            if (Parameter == "list")
            {
                data = JsonConvert.SerializeObject(ResultList);
            }

            using var writer = new StreamWriter(output.OutputStream);
            writer.Write(data);
        }

        public override Task Process(string parameter, HttpListenerContext context)
        {
            this.Parameter = parameter;
            if (parameter == "list")
            {
                ResultList = new List<PageQueryResult>();

                foreach (var query in ServerContext.PageQueryManager.GetAll())
                {
                    ResultList.Add(new PageQueryResult { 
                        id = query.Id,
                        url = query.StartingURL,
                        depth = query.Depth,
                        time = query.ToleratedAge,
                        media_regexp = query.MediaRegex,
                        page_regexp = query.PageRegex,
                    });
                }
            }

            return Task.CompletedTask;
        }
    }
}
