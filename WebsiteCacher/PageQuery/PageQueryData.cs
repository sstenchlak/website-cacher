using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteCacher
{
    public class PageQueryData
    {
        [Key]
        public int PageQueryId { get; set; }

        public virtual PageData RootPage { get; set; }
        public string URL { get; set; }

        /// <summary>
        /// Specifies how deep the scraping should go.
        /// 1 is only root page
        /// 2 is root page and its links
        /// </summary>
        public int Depth { get; set; }

        /// <summary>
        /// Specifies number of seconds for every resource downloaded under this PageQuery for how long is the resource
        /// considered as fresh. Zero means that the whole tree is always dirty. -1 is special and states for never diry.
        /// For -1 only way to update the tree is to force scrape.
        /// </summary>
        public int ToleratedAge { get; set; }

        /// <summary>
        /// Which links should be visited
        /// </summary>
        public string PageRegex { get; set; }

        /// <summary>
        /// Which media should be downloaded. You can for example select only .js and .css and therefore
        /// avoid downloading images.
        /// </summary>
        public string MediaRegex { get; set; }


        /// <summary>
        /// Whether all the tree is scraped, at least according to each node.
        /// </summary>
        /// <returns></returns>
        [NotMapped]
        public bool Scraped
        {
            get => RootPage != null ? RootPage.Scraped : false;
        }

        /// <summary>
        /// Decides whether the following PageQuery need to be updated
        /// </summary>
        /// <returns></returns>
        public bool NeedsUpdate()
        {
            return Scraped || false;
        }

        [NotMapped]
        internal PageQuery WrapperPageQuery = null;
    }
}
