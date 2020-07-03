using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace WebsiteCacher
{
    class PageManager
    {
        internal readonly DatabaseContext Context;
        internal readonly ResourceManager ResourceManager;

        public PageManager(DatabaseContext context, ResourceManager resourceManager)
        {
            Context = context;
            ResourceManager = resourceManager;
        }

        /// <summary>
        /// Returns existing Page instance or creates a completelly new Page. 
        /// </summary>
        /// <param name="url">URL address of the page</param>
        /// <param name="pageQuery">Represents scraping policy</param>
        /// <returns>Page instance represented by url and scraping policy</returns>
        public async Task<Page> GetOrCreatePage(string url, PageQuery pageQuery)
        {
            // todo: May cause problems because the first parameter is expected to be a resource
            PageData pageData = Context.Pages.Find(url, pageQuery.Id);

            if (pageData == null)
            {
                var resource = await ResourceManager.GetOrCreateResource(url);
                pageData = new PageData
                {
                    Resource = resource.DbEntity(),
                    PageQuery = pageQuery.DbEntity()
                };

                Context.Pages.Add(pageData);
                Context.SaveChanges();
            }

            return GetOrCreatePage(pageData);
        }

        public Page GetOrCreatePage(PageData pageData)
        {
            if (pageData.WrapperPage == null)
            {
                pageData.WrapperPage = new Page(pageData, this);
            }

            return pageData.WrapperPage;
        }

        /// <summary>
        /// This function scrapes all pages in <paramref name="pages"/> and returns their dependants.
        /// It is expected that all pages are on same level (having same depth <paramref name="depth"/>) and therefore
        /// the proper BFS is acomplished.
        /// </summary>
        /// <param name="pages">Set of pages to be scraped</param>
        /// <param name="depth">Depth of pages in <paramref name="pages"/>.</param>
        /// <param name="pageQuery">Scraping policy - defines tree.</param>
        /// <returns>
        /// Returns pages which depends on <paramref name="pages"/> and its depth is <paramref name="depth"/>+1, therefore
        /// it only returns undiscovered pages.
        /// </returns>
        public async Task<ISet<Page>> ScrapeDepth(ISet<Page> pages, int depth, PageQuery pageQuery, bool makeLinks)
        {
            var result = new HashSet<Page>();

            foreach(var page in pages)
            {
                result.UnionWith(await page.Scrape(depthForNewLinks: depth + 1, makeLinks: makeLinks));
            }

            return result;
        }
    }
}
