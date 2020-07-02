using System.Security.Cryptography;
using System.Linq;

namespace WebsiteCacher
{
    /// <summary>
    /// Manages page queries which are instructions how deep and which pages to scrape. These queries are created by
    /// a user and therefore there are operations such as create new PageQuery or get existing by its ID.
    /// </summary>
    class PageQueryManager
    {
        internal readonly DatabaseContext Context;
        internal readonly PageManager PageManager;

        public PageQueryManager(DatabaseContext context, PageManager pageManager)
        {
            Context = context;
            PageManager = pageManager;
        }

        /// <summary>
        /// Properly creates a new PageQuery. That means that after the setting of URL and depth, you can call .Scrape()
        /// and actually download some webpages.
        /// </summary>
        public PageQuery CreateNew()
        {
            PageQueryData data = new PageQueryData();
            Context.PageQueries.Add(data);
            return GetByData(data);
        }

        /// <summary>
        /// Returns already created PageQuery. It is possible to rescrape it or change its properties.
        /// </summary>
        /// <param name="Id">Unique numeric identifier of PageQuery</param>
        public PageQuery GetById(int Id)
        {
            PageQueryData data = Context.PageQueries.Find(Id);

            if (data == null) return null;

            return GetByData(data);
        }

        /// <summary>
        /// Helper method to obtain PageQuery from its database entity.
        /// </summary>
        /// <param name="data">Database entity</param>
        public PageQuery GetByData(PageQueryData data)
        {
            if (data.WrapperPageQuery == null)
            {
                data.WrapperPageQuery = new PageQuery(data, this);
            }

            return data.WrapperPageQuery;
        }
    }
}
