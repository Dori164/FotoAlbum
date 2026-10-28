using FotoAlbum.Models;
using Microsoft.EntityFrameworkCore;

namespace FotoAlbum.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Kep> Kep => Set<Kep>();
        public DbSet<Album> Album => Set<Album>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Album>()
                .HasMany(a => a.Kep)
                .WithOne(k => k.Album)
                .HasForeignKey(k => k.AlbumId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
