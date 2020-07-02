using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebsiteCacher
{
    /// <summary>
    /// This entity represents a node in an oriented rooted graph of www. 
    /// </summary>
    [Table("page")]
    public class PageData
    {
        /// <summary>
        /// Resource representing data of the current page. Also serves as the composite key with PageQuery.
        /// </summary>
        [Column("resource")]
        public virtual ResourceData Resource { get; set; }
        public string ResourceDataUrl { get; set; } // COMPOSITE KEY

        /// <summary>
        /// Represents strategy or policy how to scrape pages, how deep and which links.
        /// </summary>
        [Column("page_query")]
        public virtual PageQueryData PageQuery { get; set; }
        public int PageQueryId { get; set; } // COMPOSITE KEY

        /// <summary>
        /// Childrens are pages obtained by scraping current Page by specified strategy.
        /// Children pages have same PageQuery
        /// </summary>
        public virtual List<PagePageRelation> ChildrenPages { get; set; }

        /// <summary>
        /// Each page has some images, css and javascript files. The list of them is stored here.
        /// </summary>
        public virtual List<PageResourceMedia> Medias { get; set; }

        /// <summary>
        /// Represents the distance in www between this and root page.
        /// 0 - root page
        /// 1 - link to this page in in the root page
        /// </summary>
        public int Depth { get; set; } = -1;

        /// <summary>
        /// Whether the page was scraped and all the links and medias were stored in the db
        /// </summary>
        public bool Scraped { get; set; }

        /// <summary>
        /// If Scraped and all Resources have ScrapedTree
        /// </summary>
        public bool ScrapedTree { get; set; }

        [NotMapped]
        internal Page WrapperPage = null;
    }
}
