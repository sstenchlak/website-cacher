using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace WebsiteCacher
{
    class PageQuery
    {
        private readonly PageQueryData PageQueryData;
        public PageQueryData DbEntity() => PageQueryData;

        private readonly PageQueryManager PageQueryManager;

        public string StartingURL { get => PageQueryData.URL; set => PageQueryData.URL = value; }
        public string PageRegex { get => PageQueryData.PageRegex; set => PageQueryData.PageRegex = value; }
        public string MediaRegex { get => PageQueryData.MediaRegex; set => PageQueryData.MediaRegex = value; }
        public int Depth { get => PageQueryData.Depth; set => PageQueryData.Depth = value; }
        public int ToleratedAge { get => PageQueryData.ToleratedAge; set => PageQueryData.ToleratedAge = value; }
        public int Id { get => PageQueryData.PageQueryId; }

        internal PageQuery(PageQueryData pageQueryData, PageQueryManager pageQueryManager)
        {
            PageQueryData = pageQueryData;
            PageQueryManager = pageQueryManager;
        }

        /// <summary>
        /// Starts scraping whole tree by parameters set in this PageQuery. This task is asynchronous and is done
        /// when the whole tree with all resources is downloaded.
        /// </summary>
        /// <param name="force">Forcefully scrapes whole tree ignoring ToleratedAge and redownloads all the reources and pages.</param>
        public async Task Scrape(bool force = false)
        {
            Page page = null;

            if (PageQueryData.RootPage == null || PageQueryData.RootPage.Resource.URL != PageQueryData.URL)
            {
                page = PageQueryManager.PageManager.GetOrCreatePage(PageQueryData.URL, this);
                PageQueryData.RootPage = page.DbEntity();
            } else
            {
                page = PageQueryManager.PageManager.GetOrCreatePage(PageQueryData.RootPage);
            }

            page.Depth = 0;
            var dependantSet = new HashSet<Page>();
            dependantSet.Add(page);
            ISet<Page> dependant = dependantSet;

            for (int depth = 0; depth <= Depth; depth++)
            {
                dependant = await PageQueryManager.PageManager.ScrapeDepth(dependant, depth, this, depth != Depth);
            }
        }

        /// <summary>
        /// Removes page query and all its tree from the system. Resources remain untouched.
        /// </summary>
        public void Remove()
        {
            var toRemove = from page in PageQueryManager.Context.Pages where page.PageQuery == PageQueryData select page;
            foreach (var page in toRemove)
            {
                PageQueryManager.Context.Pages.Remove(page);
            }

            PageQueryManager.Context.PageQueries.Remove(PageQueryData);
        }
    }
}
