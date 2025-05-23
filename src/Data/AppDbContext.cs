using Microsoft.EntityFrameworkCore;
using MiciomaniaNamespace.Models.Manga;
using MiciomaniaNamespace.Models.Profilo;

public class AppDbContext : DbContext
{
    public DbSet<ListaManga> ListaManga { get; set; }
    public DbSet<Utente> Utente { get; set; }
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Utente>().ToTable("utenti", schema: "public");
        modelBuilder.Entity<ListaManga>().ToTable("lista_manga", schema: "info_manga");
    }

}
