using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WebsiteCacher
{
    /// <summary>
    /// Resource represents one file downloaded from the internet. It could be website or media, such as
    /// css file or image. Resource contains methods for downloading, removing and obtaining its stream
    /// from cache. Each Resource is identified by its URL which is simplified by
    /// <see cref="HtmlProcessor.SimplifyUrl(string)"/>. Resources are stored according their hash, therefore
    /// multiple resources may point to the same file.
    /// </summary>
    public class Resource
    {
        /// <summary>
        /// Database entity of the current resource
        /// </summary>
        public readonly ResourceData Data;
        public ResourceData DbEntity() => Data;

        private readonly ResourceManager Manager;

        internal Resource(ResourceData data, ResourceManager resourceManager)
        {
            this.Data = data;
            this.Manager = resourceManager;
        }

        public string URL { get => Data.URL; }
        public DateTime Updated { get => Data.Updated; }
        public string ContentType { get => Data.ContentType; }
        public bool IsDownloaded { get => Data.Hash != null; }

        /// <summary>
        /// Helper method to download resource from the internet, but not save it.
        /// </summary>
        /// <returns></returns>
        private async Task<HttpResponseMessage> Fetch()
        {
            try
            {
                var response = await Manager.HttpClient.GetAsync(this.Data.URL);

                if (response.IsSuccessStatusCode)
                {
                    return response;
                }

            }
            catch (Exception) { } // InvalidOperationException OR System.Net.Http.HttpRequestException OR Something else?

            return null;
        }

        /// <summary>
        /// Fetches the resource from the Internet and caches it into the local file system
        /// </summary>
        /// <returns>If the operation was successfull</returns>
        public async Task<bool> Download()
        {
            bool status = false;
            await Task.Run(async () =>
            {
                var response = await Fetch();
                if (response != null)
                {
                    status = true;
                    string hash = await Manager.HashSolver.ComputeHash(response.Content);

                    await Manager.Storage.Add(hash, response.Content);
                    Data.Hash = hash;
                    Data.Updated = DateTime.Now;
                    Data.ContentType = response.Content.Headers.ContentType?.ToString();
                }
            });
            await Manager.Context.SaveChangesAsync();

            return status;
        }

        /// <summary>
        /// Removes itself without the file stored in storage, because multiple resources may point here.
        /// </summary>
        public void Remove()
        {
            Manager.Context.Resources.Remove(Data);
        }

        /// <summary>
        /// Gets the cached data from the filesystem
        /// </summary>
        /// <returns></returns>
        public Stream Get()
        {
            return Manager.Storage.Get(Data.Hash);
        }
    }
}
