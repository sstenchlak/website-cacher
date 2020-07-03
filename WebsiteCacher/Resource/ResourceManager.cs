using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebsiteCacher
{
    /// <summary>
    /// Deals with downloading resources and caching them properly.
    /// </summary>
    public class ResourceManager
    {
        internal readonly DatabaseContext Context;
        internal readonly Storage Storage;
        internal readonly HashSolver HashSolver;
        internal readonly HttpClient HttpClient = new HttpClient();

        public ResourceManager(DatabaseContext context, Storage storage, HashSolver hashSolver)
        {
            Context = context;
            Storage = storage;
            HashSolver = hashSolver;
        }

        public async Task<Resource> GetOrCreateResource(string url)
        {
            var query = from r in this.Context.Resources where (r.URL == url) select r;
            var resourceData = query.FirstOrDefault();

            // Create a new resource if not exists
            if (resourceData == null)
            {
                resourceData = new ResourceData { URL = url };
                Context.Resources.Add(resourceData);
                Context.SaveChanges();
            }

            return GetOrCreateResource(resourceData);
        }

        public Resource GetResource(string url)
        {
            var query = from r in this.Context.Resources where (r.URL == url) select r;
            var resourceData = query.FirstOrDefault();
            if (resourceData == null) return null;
            return GetOrCreateResource(resourceData);
        }

        public Resource GetOrCreateResource(ResourceData resourceData)
        {
            if (resourceData.WrapperResource == null)
            {
                resourceData.WrapperResource = new Resource(resourceData, this);
            }

            return resourceData.WrapperResource;
        }
    }
}
