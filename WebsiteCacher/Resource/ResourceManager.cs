using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
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

        /// <summary>
        /// Removes those resources from database which are not connected to any page.
        /// First, you should call PageGC() and then ResouceGC().
        /// </summary>
        public void ResourceGC(out int removed, out int kept)
        {
            var existingResources = new HashSet<ResourceData>();
            foreach (var page in Context.Pages)
            {
                if (page.Resource != null) existingResources.Add(page.Resource);
                if (page.Medias != null) foreach (var relation in page.Medias) existingResources.Add(relation.TargetResource);
            }

            removed = 0;
            kept = 0;

            foreach (var resource in Context.Resources)
            {
                if (!existingResources.Contains(resource))
                {
                    GetOrCreateResource(resource).Remove();
                    removed++;
                } else
                {
                    kept++;
                }
            }

            Context.SaveChanges();
        }
    }
}
