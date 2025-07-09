using Models.Manga;
using Models.Canzoni;
using Models.Proposte;
using Microsoft.EntityFrameworkCore;

namespace Data.ApplicationDbContext
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Manga> Manga { get; set; }
        public DbSet<Canzone> Canzone { get; set; }
        public DbSet<Proposta> Proposta { get; set; }

        // Costruttore
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }
    }
}
