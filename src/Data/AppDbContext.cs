using Microsoft.EntityFrameworkCore;
using MiciomaniaNamespace.Models;

public class AppDbContext : DbContext
{
    public DbSet<ListaManga> ListaManga { get; set; }
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ListaManga>().ToTable("lista_manga", schema: "info_manga");
    }

}
