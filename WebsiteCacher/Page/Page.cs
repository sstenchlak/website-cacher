using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebsiteCacher
{
    /// <summary>
    /// Page represents hyper-text document, such as HTML document, which belongs to one specific PageQuery.
    /// That means, that single website may occur multiple times if multiple PageQueries requests it. The purpose
    /// of this is that different PageQueries may have different rules which subpages needs to be scraped.
    /// </summary>
    class Page
    {
        /// <summary>
        /// Link to database entity in Entity Core Framework
        /// </summary>
        private readonly PageData PageData;

        /// <summary>
        /// Database entity in Entity Core Framework
        /// </summary>
        /// <returns></returns>
        public PageData DbEntity() => PageData;

        private readonly PageManager PageManager;

        /// <summary>
        /// Constructor for Page. Meant to be called by PageManager only.
        /// </summary>
        internal Page(PageData pageData, PageManager pageManager)
        {
            PageData = pageData;
            PageManager = pageManager;
        }

        /// <summary>
        /// URL of the page from where it was downloaded
        /// </summary>
        public string Url { get => PageData.Resource.URL; }

        /// <summary>
        /// Number representing the shortest path from PageQuery.
        /// 0 - The page to which PageQuery points
        /// </summary>
        internal int Depth { get => PageData.Depth; set => PageData.Depth = value; }

        /// <summary>
        /// Parent PageQuery, root of this page "tree"
        /// </summary>
        private PageQuery PageQuery { get => PageData.PageQuery.WrapperPageQuery; }

        /// <summary>
        /// Performs scraping on this page.
        /// Meant to be used by PageQuery.
        /// </summary>
        /// <param name="makeLinks">Whether to search for links on this page or just ignore them.</param>
        /// <returns>
        /// Enumerable collection of Pages which depends on this page and its depth is <paramref name="depthForNewLinks"/>.
        /// If <paramref name="makeLinks"/> is false than is empty.
        /// </returns>
        public async Task<IEnumerable<Page>> Scrape(bool forceMedia = false, bool forceLinks = false, bool makeLinks = true, int depthForNewLinks = -1)
        {
            Console.WriteLine($"Scraping <{Url}>");

            var resource = await PageManager.ResourceManager.GetOrCreateResource(PageData.Resource.URL);
            bool result = await DownloadResourceByTime(resource, false);

            // List of pages which depends on this one (under the specific PageQuery)
            IEnumerable<Page> dependants = Enumerable.Empty<Page>();

            if (result)
            {
                var processor = new HtmlProcessor(resource.Get(), PageData.Resource.URL);
                processor.Load();
                IPageScraper scraper = new SimplePageScraper { 
                    Data = resource,
                    Processor = processor,
                    PageQuery = PageQuery
                };
                var links = scraper.ScrapeLinks();
                var media = scraper.ScrapeMedia();

                await ProcessMedia(media, false);

                if (makeLinks) dependants = await ProcessLinks(links, false, depthForNewLinks);

                PageManager.ResourceManager.Context.SaveChanges();
            }

            return dependants;
        }

        /// <summary>
        /// Downloads a resource if needed. That means if the resource is not downloaded yet, if is too old or if force scraping is on.
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="force">Force download (always)</param>
        /// <returns>If the resource is successfully cached.</returns>
        private async Task<bool> DownloadResourceByTime(Resource resource, bool force = false)
        {
            if (force || !resource.IsDownloaded || (DateTime.Now - resource.Updated).TotalSeconds > PageQuery.ToleratedAge)
            {
                Console.WriteLine($"    Downloading <{resource.URL}>");
                return await resource.Download();
            } else
            {
                return true;
            }
        }

        /// <summary>
        /// Helper function to process media links obtained from the page.
        /// </summary>
        /// <param name="newMedia">List of absolute urls to media by current policy</param>
        private async Task ProcessMedia(ISet<string> newMedia, bool force)
        {
            PageManager.ResourceManager.Context.Entry(PageData).Collection(b => b.Medias).Load();

            // We need to update the old list of linked medias
            var newList = new List<PageResourceMedia>();

            // First, remove not-used media and from list remove used media
            foreach (var existingMedia in PageData.Medias)
            {
                PageManager.ResourceManager.Context.Entry(existingMedia).Reference(b => b.TargetResource).Load();

                if (newMedia.Contains(existingMedia.TargetResource.URL))
                {
                    newMedia.Remove(existingMedia.TargetResource.URL);
                    newList.Add(existingMedia);
                }
            }

            // Second, add new media
            foreach (var m in newMedia)
            {
                var newResource = await PageManager.ResourceManager.GetOrCreateResource(m);
                newList.Add(new PageResourceMedia { TargetResource = newResource.Data });
            }

            // Store data and update database
            PageData.Medias = newList;
            await PageManager.Context.SaveChangesAsync();

            // Download resources
            foreach (var relation in newList)
            {
                var resource = PageManager.ResourceManager.GetOrCreateResource(relation.TargetResource);
                await DownloadResourceByTime(resource, force);
            }
        }

        /// <summary>
        /// Helper function to process page links obtained from this page. It unlinks old ones and links or creates new ones.
        /// Unlike the ProcessMedia, this method DOES NOT scrape those links. It only connects them. Resason is that
        /// we want to traverse the www tree in BFS order instead of DFS where recursion can be used.
        /// </summary>
        /// <param name="newLinks">List of absolute urls to websites by current policy</param>
        /// <returns>
        /// List of pages which depends on this one (under the current PageQuery policy) and were discovered for the first time.
        /// That means its <code>depth</code> is exactly <code><see cref="Depth"/>+1</code>
        /// </returns>
        private async Task<IEnumerable<Page>> ProcessLinks(ISet<string> newLinks, bool force, int DepthForNewLinks)
        {
            PageManager.ResourceManager.Context.Entry(PageData).Collection(b => b.ChildrenPages).Load();

            var newList = new List<PagePageRelation>();

            // First copy already existing pages which are in newLinks
            foreach (var actualChildren in PageData.ChildrenPages)
            {
                PageManager.ResourceManager.Context.Entry(actualChildren).Reference(b => b.TargetPage).Load();

                if (newLinks.Contains(actualChildren.TargetPage.Resource.URL))
                {
                    newLinks.Remove(actualChildren.TargetPage.Resource.URL);
                    newList.Add(actualChildren);
                }
            }

            // Second, create new Pages
            foreach (var m in newLinks)
            {
                var page = await PageManager.GetOrCreatePage(m, PageQuery);
                newList.Add(new PagePageRelation { TargetPage = page.DbEntity() });
            }

            // Set depths
            foreach (var relation in newList)
            {
                var link = relation.TargetPage;
                if ((link.Depth == -1 || link.Depth > DepthForNewLinks) && DepthForNewLinks != -1)
                {
                    link.Depth = DepthForNewLinks;
                }
            }

            PageData.ChildrenPages = newList;
            await PageManager.Context.SaveChangesAsync();

            return from relation in newList where relation.TargetPage.Depth == DepthForNewLinks select PageManager.GetOrCreatePage(relation.TargetPage);
        }
    }
}
