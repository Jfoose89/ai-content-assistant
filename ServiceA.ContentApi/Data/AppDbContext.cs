using Microsoft.EntityFrameworkCore;
using ServiceA.ContentApi.Entities;

namespace ServiceA.ContentApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<DndGeneratedContent> Contents => Set<DndGeneratedContent>();
    }
}
