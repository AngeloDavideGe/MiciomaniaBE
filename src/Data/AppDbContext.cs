using Miciomania.Models.Proposte;
using Miciomania.Models.Manga;
using Microsoft.EntityFrameworkCore;

namespace Miciomania.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Manga> Manga { get; set; }
        public DbSet<Proposte> Proposte { get; set; }

        // Costruttore
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }
    }
}
