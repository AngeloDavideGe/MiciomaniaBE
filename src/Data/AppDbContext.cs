using Microsoft.EntityFrameworkCore;
using UserModels;
using TweetModels;
using GiocatoreModels;
using ListaMangaModels;
using MangaUtenteModels;
using SquadraModels;
using AdminModels;

namespace Data.ApplicationDbContext
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        // utenti_schema
        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Tweet> Tweets { get; set; }

        // manga_schema
        public DbSet<MangaUtente> MangaUtenti { get; set; }
        public DbSet<ListaManga> ListaManga { get; set; }

        // squadre_schema
        public DbSet<Squadra> Squadre { get; set; }
        public DbSet<Giocatore> Giocatori { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Mapping tabelle negli schemi
            modelBuilder.Entity<User>().ToTable("utenti", "utenti_schema");
            modelBuilder.Entity<Admin>().ToTable("admin", "utenti_schema");
            modelBuilder.Entity<Tweet>().ToTable("tweet", "utenti_schema");

            modelBuilder.Entity<MangaUtente>().ToTable("mangautente", "manga_schema");
            modelBuilder.Entity<ListaManga>().ToTable("listamanga", "manga_schema");

            modelBuilder.Entity<Squadra>().ToTable("squadre", "squadre_schema");
            modelBuilder.Entity<Giocatore>().ToTable("giocatori", "squadre_schema");

            // Chiavi primarie composite o FK se serve
            modelBuilder.Entity<Admin>()
                .HasKey(a => a.idutente);

            modelBuilder.Entity<MangaUtente>()
                .HasKey(m => m.idutente);

            modelBuilder.Entity<Giocatore>()
                .HasKey(g => g.idutente);

            // Relazioni (opzionale)
            modelBuilder.Entity<Admin>()
                .HasOne<User>()
                .WithOne()
                .HasForeignKey<Admin>(a => a.idutente)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MangaUtente>()
                .HasOne<User>()
                .WithOne()
                .HasForeignKey<MangaUtente>(m => m.idutente)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Giocatore>()
                .HasOne<User>()
                .WithOne()
                .HasForeignKey<Giocatore>(g => g.idutente)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
