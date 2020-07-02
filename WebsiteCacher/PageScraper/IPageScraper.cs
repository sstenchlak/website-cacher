using System.Collections.Generic;

namespace WebsiteCacher
{
    interface IPageScraper
    {
        // Represents HTML page
        HtmlProcessor Processor { get;  set; }

        // Resource data
        Resource Data { get; set; }

        // Scraping policy
        PageQuery PageQuery { get; set; }


        public ISet<string> ScrapeLinks();
        public ISet<string> ScrapeMedia();
    }
}
