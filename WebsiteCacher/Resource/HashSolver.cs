using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace WebsiteCacher
{
    /// <summary>
    /// Class computing hash of downloaded resources. It can be extended to ignore timestamps in HTML files which breaks meaning of hashing.
    /// </summary>
    public class HashSolver
    {
        public async Task<string> ComputeHash(HttpContent content)
        {
            string hash;
            using (var md5 = MD5.Create())
            {
                var h = md5.ComputeHash(await content.ReadAsStreamAsync());
                hash = BitConverter.ToString(h).Replace("-", String.Empty).ToLower();
            }
            return hash;
        }
    }
}
