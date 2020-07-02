using System.IO;

namespace WebsiteCacher
{
    /// <summary>
    /// This class modifies the webpage before it is sent to the client.
    /// </summary>
    class PageModifier
    {
        private readonly Stream Input;
        private readonly string Url;

        private HtmlProcessor Processor;

        public PageModifier(Stream input, string URL)
        {
            this.Input = input;
            this.Url = URL;
        }

        public void Process()
        {
            this.Processor = new HtmlProcessor(this.Input, this.Url);
            this.Processor.Load();

            // Process HREF attributes
            var links = this.Processor.Document.DocumentNode.SelectNodes("//*[@href]");

            foreach (var link in links)
            {
                link.Attributes["href"].Value = this.FixLink(link.Attributes["href"].Value);
            }

            // Process SRC attributes
            links = this.Processor.Document.DocumentNode.SelectNodes("//*[@src]");

            foreach (var link in links)
            {
                link.Attributes["src"].Value = this.FixLink(link.Attributes["src"].Value);
            }
        }

        private string FixLink(string link)
        {
            var absolute = this.Processor.GetAbsoluteLink(link);
            return $"/r/{absolute}";
        }

        public Stream GetResult()
        {
            return this.Processor.GetResult();
        }
    }
}
