using System.ComponentModel.DataAnnotations;

namespace WebsiteCacher
{
    public class PageResourceMedia
    {
        [Required]
        public int PageResourceMediaId { get; set; }
        [Required]
        public virtual ResourceData TargetResource { get; set; }
        [Required]
        public virtual PageData SourcePage { get; set; }
    }

    public class PagePageRelation
    {
        [Required]
        public int PagePageRelationId { get; set; }
        [Required]
        public virtual PageData TargetPage { get; set; }
    }
}
