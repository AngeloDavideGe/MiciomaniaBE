using Microsoft.EntityFrameworkCore;
using UserModels;
using TweetModels;
using GiocatoreModels;
using MangaUtenteModels;
using SquadraModels;
using AdminModels;
using MangaModels;
using CanzoniMiciomaniaModels;
using CanzoniUtenteModels;
using MangaMiciomaniaModels;
using MangaUtenteParModels;

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
        public DbSet<MangaClass> ListaManga { get; set; }

        // squadre_schema
        public DbSet<Squadra> Squadre { get; set; }
        public DbSet<Giocatore> Giocatori { get; set; }

        // parodie_schema
        public DbSet<CanzoniMiciomania> CanzoniMicio { get; set; }
        public DbSet<CanzoniUtente> CanzoniUser { get; set; }
        public DbSet<MangaMiciomania> MangaMicio { get; set; }
        public DbSet<MangaUtentePar> MangaUserPar { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Mapping tabelle negli schemi
            modelBuilder.Entity<User>().ToTable("utenti", "utenti_schema");
            modelBuilder.Entity<Admin>().ToTable("admin", "utenti_schema");
            modelBuilder.Entity<Tweet>().ToTable("tweet", "utenti_schema");

            modelBuilder.Entity<MangaUtente>().ToTable("mangautente", "manga_schema");
            modelBuilder.Entity<MangaClass>().ToTable("listamanga", "manga_schema");

            modelBuilder.Entity<Squadra>().ToTable("squadre", "squadre_schema");
            modelBuilder.Entity<Giocatore>().ToTable("giocatori", "squadre_schema");

            modelBuilder.Entity<CanzoniMiciomania>().ToTable("canzonimiciomania", "parodie_schema");
            modelBuilder.Entity<CanzoniUtente>().ToTable("canzoniutente", "parodie_schema");
            modelBuilder.Entity<MangaMiciomania>().ToTable("mangamiciomania", "parodie_schema");
            modelBuilder.Entity<MangaUtentePar>().ToTable("mangautentepar", "parodie_schema");

            modelBuilder.Entity<Tweet>(entity =>
            {
                entity.HasKey(t => t.id);

                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(t => t.idUtente)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Admin>(entity =>
            {
                entity.HasKey(a => a.idUtente);

                entity.HasOne<User>()
                    .WithOne()
                    .HasForeignKey<Admin>(a => a.idUtente)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MangaUtente>(entity =>
            {
                entity.HasKey(m => m.idUtente);

                entity.HasOne<User>()
                    .WithOne()
                    .HasForeignKey<MangaUtente>(m => m.idUtente)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Giocatore>(entity =>
            {
                entity.HasKey(g => g.idUtente);

                entity.HasOne<User>()
                    .WithOne()
                    .HasForeignKey<Giocatore>(g => g.idUtente)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CanzoniMiciomania>(entity =>
            {
                entity.HasKey(g => g.idUtente);

                entity.HasOne<User>()
                    .WithOne()
                    .HasForeignKey<CanzoniMiciomania>(g => g.idUtente)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CanzoniUtente>(entity =>
            {
                entity.HasKey(g => g.idUtente);

                entity.HasOne<User>()
                    .WithOne()
                    .HasForeignKey<CanzoniUtente>(g => g.idUtente)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MangaMiciomania>(entity =>
            {
                entity.HasKey(g => g.idUtente);

                entity.HasOne<User>()
                    .WithOne()
                    .HasForeignKey<MangaMiciomania>(g => g.idUtente)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MangaUtentePar>(entity =>
            {
                entity.HasKey(g => g.idUtente);

                entity.HasOne<User>()
                    .WithOne()
                    .HasForeignKey<MangaUtentePar>(g => g.idUtente)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
