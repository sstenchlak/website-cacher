using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace WebsiteCacher
{
    /// <summary>
    /// Represents database entity
    /// </summary>
    [Table("Resource")]
    public class ResourceData
    {
        [Key]
        public string URL { get; set; }

        /// <summary>
        /// Hash computed from the resource data. It is used to index the file in the filesystem.
        /// </summary>
        public string Hash { get; set; }

        public DateTime Updated { get; set; }

        /// <summary>
        /// Represents HTTP Content-Type header property
        /// </summary>
        public string ContentType { get; set; }

        [NotMapped]
        internal Resource WrapperResource = null;
    }
}
