using Microsoft.EntityFrameworkCore;
using MiciomaniaNamespace.Models.Manga;
using MiciomaniaNamespace.Models.Profilo;

public class CopyDbContext : DbContext
{
    public DbSet<Pubblicazioni> Pubblicazioni { get; set; }
    public DbSet<MangaUtente> MangaUtente { get; set; }
    public CopyDbContext(DbContextOptions<CopyDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Pubblicazioni>().ToTable("pubblicazioni", schema: "public");
        modelBuilder.Entity<MangaUtente>().ToTable("manga_utente", schema: "info_manga");
    }

}
