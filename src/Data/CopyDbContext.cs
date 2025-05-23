using Microsoft.EntityFrameworkCore;
using MiciomaniaNamespace.Models;

public class CopyDbContext : DbContext
{
    public DbSet<MangaUtente> MangaUtente { get; set; }
    public CopyDbContext(DbContextOptions<CopyDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MangaUtente>().ToTable("manga_utente", schema: "info_manga");
    }

}
