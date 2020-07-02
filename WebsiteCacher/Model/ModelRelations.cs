namespace WebsiteCacher
{
    public class PageResourceMedia
    {
        public int PageResourceMediaId { get; set; }
        public virtual ResourceData TargetResource { get; set; }
    }

    public class PagePageRelation
    {
        public int PagePageRelationId { get; set; }
        public virtual PageData TargetPage { get; set; }
    }
}
