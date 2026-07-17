using Microsoft.EntityFrameworkCore;
using UserModels;
using TweetModels;
using GiocatoreModels;
using MangaUtenteModels;
using SquadraModels;
using AdminModels;
using MangaModels;
using InterazioniModels;
using CronModels;
using MangaMiciomaniaModels;

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
        public DbSet<InterazioniDB> Interazioni { get; set; }

        // manga_schema
        public DbSet<MangaUtente> MangaUtenti { get; set; }
        public DbSet<MangaClass> ListaManga { get; set; }
        public DbSet<MangaMiciomania> MangaMiciomania { get; set; }

        // squadre_schema
        public DbSet<Squadra> Squadre { get; set; }
        public DbSet<Giocatore> Giocatori { get; set; }

        public DbSet<CronUtenti> CronUtenti { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("utenti", "utenti_schema");
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.id);
            });

            modelBuilder.Entity<Admin>().ToTable("admin", "utenti_schema");
            modelBuilder.Entity<Admin>(entity =>
            {
                entity.HasKey(a => a.idUtente);

                entity.HasOne<User>()
                    .WithOne()
                    .HasForeignKey<Admin>(a => a.idUtente)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Tweet>().ToTable("tweet", "utenti_schema");
            modelBuilder.Entity<Tweet>(entity =>
            {
                entity.HasKey(t => t.id);

                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(t => t.idUtente)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MangaClass>().ToTable("listamanga", "manga_schema");

            modelBuilder.Entity<MangaMiciomania>().ToTable("mangamiciomania", "manga_schema");
            modelBuilder.Entity<MangaMiciomania>(entity =>
            {
                entity.HasKey(t => t.id);

                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(t => t.autore)
                    .OnDelete(DeleteBehavior.Cascade);
            });


            modelBuilder.Entity<InterazioniDB>().ToTable("interazioni", "utenti_schema");

            modelBuilder.Entity<MangaUtente>().ToTable("mangautente", "manga_schema");

            modelBuilder.Entity<Squadra>().ToTable("squadre", "squadre_schema");
            modelBuilder.Entity<Giocatore>().ToTable("giocatori", "squadre_schema");

            modelBuilder.Entity<CronUtenti>().ToTable("cron_utenti", "crono_schema");




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

            modelBuilder.Entity<InterazioniDB>(entity =>
            {
                entity.HasKey(g => g.id);

                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(i => i.user1)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(i => i.user2)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CronUtenti>(entity =>
            {
                entity.HasKey(g => g.id);

                entity.HasOne<User>()
                    .WithOne()
                    .HasForeignKey<CronUtenti>(g => g.idUtente)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
