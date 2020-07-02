using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Cache;
using System.Security.Cryptography;
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

        public string Url { get => PageData.Resource.URL; }
        internal int Depth { get => PageData.Depth; set => PageData.Depth = value; }
        private PageQuery PageQuery { get => PageData.PageQuery.WrapperPageQuery; }

        /// <summary>
        /// Performs scrape on this page.
        /// Meant to be used by PageQuery
        /// </summary>
        /// <param name="makeLinks">Whether to search for links on this page or just ignore them.</param>
        /// <returns>
        /// Enumerable collection of Pages which depends on this page and its depth is <paramref name="depthForNewLinks"/>.
        /// If <paramref name="makeLinks"/> is false than is empty.
        /// </returns>
        public async Task<IEnumerable<Page>> Scrape(bool forceMedia = false, bool forceLinks = false, bool makeLinks = true, int depthForNewLinks = -1)
        {
            Console.WriteLine(Url);

            var resource = PageManager.ResourceManager.GetOrCreateResource(PageData.Resource.URL);

            bool result = await DownloadResourceByTime(resource, false);

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

                //foreach (var m in media) Console.WriteLine(m);

                await ProcessMedia(media, false);


                if (makeLinks) dependants = ProcessLinks(links, false, depthForNewLinks);

                PageManager.ResourceManager.Context.SaveChanges();
            }

            return dependants;
        }

        /// <summary>
        /// Downloads a resource if needed. That means if the resource is not downloaded yet, if is too old or if force scraping is on.
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="force">Force download (always)</param>
        /// <returns></returns>
        private async Task<bool> DownloadResourceByTime(Resource resource, bool force = false)
        {
            if (force || !resource.IsDownloaded || (DateTime.Now - resource.Updated).TotalSeconds > PageQuery.ToleratedAge)
            {
                Console.WriteLine("    " + resource.URL);
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
                var newResource = PageManager.ResourceManager.GetOrCreateResource(m);
                newList.Add(new PageResourceMedia { TargetResource = newResource.Data });
            }

            PageData.Medias = newList;

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
        private IEnumerable<Page> ProcessLinks(ISet<string> newLinks, bool force, int DepthForNewLinks)
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
                var page = PageManager.GetOrCreatePage(m, PageQuery);
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

            return from relation in newList where relation.TargetPage.Depth == DepthForNewLinks select PageManager.GetOrCreatePage(relation.TargetPage);
        }
    }
}
