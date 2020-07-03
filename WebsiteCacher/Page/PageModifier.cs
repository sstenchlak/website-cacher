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

            if (links != null)
            {
                foreach (var link in links)
                {
                    var address = link.Attributes["href"].Value;
                    link.Attributes["href"].Value = this.FixLink(address);
                    if (link.Name == "a")
                    {
                        link.SetAttributeValue("data-websitecacher-link", this.Processor.GetAbsoluteLink(address));
                    }
                }
            }

            // Process SRC attributes
            links = this.Processor.Document.DocumentNode.SelectNodes("//*[@src]");

            if (links != null)
            {
                foreach (var link in links)
                {
                    link.Attributes["src"].Value = this.FixLink(link.Attributes["src"].Value);
                }
            }

            // Insert into head
            var head = this.Processor.Document.DocumentNode.SelectSingleNode("//head");
            if (head != null)
            {
                var script = this.Processor.Document.CreateElement("script");
                script.SetAttributeValue("src", "/website-cacher://static-content/webInjector.js");
                head.AppendChild(script);

                var style = this.Processor.Document.CreateElement("link");
                style.SetAttributeValue("rel", "stylesheet");
                style.SetAttributeValue("type", "text/css");
                style.SetAttributeValue("href", "/website-cacher://static-content/webInjector.css");
                head.AppendChild(style);
            }
        }

        private string FixLink(string link)
        {
            var absolute = this.Processor.GetAbsoluteLink(link);
            return $"/{absolute}";
        }

        public Stream GetResult()
        {
            return this.Processor.GetResult();
        }
    }
}
