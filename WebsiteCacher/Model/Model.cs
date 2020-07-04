using Microsoft.EntityFrameworkCore;

namespace WebsiteCacher
{
    public class DatabaseContext : DbContext
    {
        public DbSet<PageQueryData> PageQueries { get; set; }
        public DbSet<ResourceData> Resources { get; set; }
        public DbSet<PageData> Pages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseLazyLoadingProxies().UseSqlite("Data Source=site_database.db");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PageData>()
                .HasMany(b => b.Medias)
                .WithOne(b => b.SourcePage);

            modelBuilder.Entity<PageData>()
                .HasMany(b => b.ChildrenPages)
                .WithOne();

            modelBuilder.Entity<PageData>()
                .HasOne(p => p.Resource)
                .WithMany()
                .HasForeignKey(p => p.ResourceDataUrl);

            modelBuilder.Entity<PageData>()
                .HasOne(p => p.PageQuery)
                .WithMany()
                .HasForeignKey(p => p.PageQueryId);

            modelBuilder.Entity<PageData>()
                .HasKey(c => new { c.ResourceDataUrl, c.PageQueryId });
        }
    }
}