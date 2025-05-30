using Miciomania.Models.Manga;
using Miciomania.Models.Canzoni;
using Miciomania.Models.Proposte;
using Microsoft.EntityFrameworkCore;

namespace Miciomania.Data
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
