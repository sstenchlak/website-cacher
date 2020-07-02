using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WebsiteCacher
{
    class SimplePageScraper : IPageScraper
    {
        public HtmlProcessor Processor { get; set; }
        public Resource Data { get; set; }
        public PageQuery PageQuery { get; set; }

        public ISet<string> ScrapeLinks()
        {
            if (this.Processor == null || this.Data == null || this.PageQuery == null)
            {
                throw new InvalidOperationException();
            }

            var result = new HashSet<string>();

            // Prepare regex
            Regex rx = new Regex(this.PageQuery.PageRegex);

            var links = this.Processor.Document.DocumentNode.SelectNodes("//a[@href]");

            if (links != null)
            {
                foreach (var link in links)
                {
                    var absoluteLink = this.Processor.GetAbsoluteLink(link.Attributes["href"].DeEntitizeValue);
                    if (rx.IsMatch(absoluteLink))
                    {
                        result.Add(absoluteLink);
                    }
                }
            }

            return result;
        }

        public ISet<string> ScrapeMedia()
        {
            if (this.Processor == null || this.Data == null || this.PageQuery == null)
            {
                throw new InvalidOperationException();
            }

            var result = new HashSet<string>();

            // Prepare regex
            Regex rx = new Regex(this.PageQuery.MediaRegex);

            var hrefs = this.Processor.Document.DocumentNode.SelectNodes("//link[@href]");

            if (hrefs != null)
            {
                foreach (var link in hrefs)
                {
                    var absoluteLink = this.Processor.GetAbsoluteLink(link.Attributes["href"].DeEntitizeValue);
                    if (rx.IsMatch(absoluteLink))
                    {
                        result.Add(absoluteLink);
                    }
                }
            }
            
            var srcs = this.Processor.Document.DocumentNode.SelectNodes("//script[@src] | //stylesheet[@src] | //img[@src]");

            if (srcs != null)
            {
                foreach (var link in srcs)
                {
                    var absoluteLink = this.Processor.GetAbsoluteLink(link.Attributes["src"].DeEntitizeValue);
                    if (rx.IsMatch(absoluteLink))
                    {
                        result.Add(absoluteLink);
                    }
                }
            }
            
            return result;
        }
    }
}
