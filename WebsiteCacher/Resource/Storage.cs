using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebsiteCacher
{
    /// <summary>
    /// This class takes care about storing resources such as downloaded webpages and images into the local file system.
    /// </summary>
    public class Storage
    {
        private string StorageLocation;

        public Storage(string storage)
        {
            this.StorageLocation = storage;
        }

        private string GetFileLocation(string hash)
        {
            return Path.Combine(this.StorageLocation, hash.Substring(0, 2), hash);
        }

        public bool Contains(string hash)
        {
            return File.Exists(this.GetFileLocation(hash));
        }

        public async Task Add(string hash, HttpContent content)
        {
            if (Contains(hash)) return;

            var location = this.GetFileLocation(hash);
            Directory.CreateDirectory(Path.GetDirectoryName(location));
            using (var fs = new FileStream(location, FileMode.Create))
            {
                await content.CopyToAsync(fs);
                fs.Close();
            }
        }

        /// <summary>
        /// Removes file from storage and directory if empty.
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public bool Remove(string hash)
        {
            var location = this.GetFileLocation(hash);
            try
            {
                File.Delete(location);
                var dir = Path.GetDirectoryName(location);
                if (Directory.EnumerateFileSystemEntries(dir).Count() == 0)
                {
                    Directory.Delete(dir);
                }
            } catch (Exception)
            {
                return false;
            }
            return true;
        }

        public Stream Get(string hash)
        {
            return new FileStream(this.GetFileLocation(hash), FileMode.Open, FileAccess.Read);
        }
    }
}
