using Flurl;
using HtmlAgilityPack;
using System;
using System.IO;

namespace WebsiteCacher
{
    /// <summary>
    /// This class modifies the HTML page before it is served to a client.
    /// </summary>
    class HtmlProcessor
    {
        // http://example.com/sub-folder/index.html?value=1
        private readonly string PageUrl;

        public HtmlDocument Document;
        private readonly Stream Input;

        // computed http://example.com/
        private string PageDomain = "";

        // computed http://example.com/sub-folder/
        private string PageBase = "";

        public HtmlProcessor(Stream input, string URL)
        {
            this.Input = input;
            this.PageUrl = URL;
        }

        public void Load()
        {
            this.Document = new HtmlDocument();
            this.Document.Load(this.Input);

            var url = new Uri(this.PageUrl);
            this.PageDomain = url.GetLeftPart(System.UriPartial.Authority);

            var baseLink = this.Document.DocumentNode.SelectSingleNode("//base[@href]")?.Attributes["href"].Value;

            if (baseLink != null)
            {
                if (Uri.IsWellFormedUriString(baseLink, UriKind.Absolute))
                {
                    this.PageBase = Url.Combine(baseLink, baseLink);
                } else
                {
                    this.PageBase = Url.Combine(this.PageDomain, baseLink);
                }
            } else
            {
                this.PageBase = (new Uri(url, ".")).OriginalString;
            }
        }

        public Stream GetResult()
        {
            // Convert to stream
            var output = new MemoryStream();
            var writer = new StreamWriter(output);
            this.Document.Save(writer);
            writer.Flush();
            output.Position = 0;
            return output;
        }

        /// <summary>
        /// Generates absolute URL link from the link in the document.
        /// For example: ?value=1 -> http://example.com/sub-folder/index.html?value=1
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        public string GetAbsoluteLink(string link)
        {
            // For now, there is a simple logic to determine whether the link is relative or absolute

            if (link.StartsWith('/'))
            {
                return Url.Combine(this.PageDomain, link);
            }

            if (Uri.IsWellFormedUriString(link, UriKind.Absolute))
            {
                return link;
            } else
            {
                return Url.Combine(this.PageBase, link);
            }
        }
    }
}
