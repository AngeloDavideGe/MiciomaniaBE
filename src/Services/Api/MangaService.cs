using CacheName;
using Data.ApplicationDbContext;
using MangaForms;
using MangaModels;
using MangaUtenteModels;
using MangaViews;
using Microsoft.EntityFrameworkCore;
using TaskOption;

namespace Manga.Services
{
    public class MangaService
    {
        private readonly AppDbContext _context;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly CacheService _cacheService;

        public MangaService(
            AppDbContext context,
            IDbContextFactory<AppDbContext> contextFactory,
            CacheService cacheService)
        {
            _context = context;
            _contextFactory = contextFactory;
            _cacheService = cacheService;
        }

        public async Task<MangaUtenteGet> GetMangaUtente(string idUtente)
        {
            await using var context = _contextFactory.CreateDbContext();

            if (string.IsNullOrEmpty(idUtente))
            {
                return new MangaUtenteGet("", "", "");
            }

            return await context.MangaUtenti
                .Where((MangaUtente m) => m.idUtente == idUtente)
                .Select((MangaUtente m) => new MangaUtenteGet(m.preferiti, m.letti, m.completati))
                .FirstOrDefaultAsync()
                .ContinueWith((Task<MangaUtenteGet?> task) => task.Result ?? new MangaUtenteGet("", "", ""));
        }

        public async Task<List<MangaClass>> GetAllMangaCache()
        {
            return await _cacheService.GetOrCreate(new CacheOptions<List<MangaClass>>
            {
                Task = () => _context.ListaManga.ToListAsync(),
                NomeCache = "AllManga",
                DurataCache = TimeSpan.FromHours(2)
            });
        }

        public Task AggiungiManga(MangaClass mangaForm)
        {
            var newManga = new MangaClass
            {
                nome = mangaForm.nome,
                autore = mangaForm.autore,
                genere = mangaForm.genere,
                copertina = mangaForm.copertina,
                path = mangaForm.path,
                completato = mangaForm.completato
            };

            _context.ListaManga.Add(newManga);
            return _context.SaveChangesAsync();
        }

        public async Task UpsertManga(string idUtente, MangaUtenteForm nuoviManga)
        {
            MangaUtente? mangaUtente = await _context.MangaUtenti
                .FirstOrDefaultAsync(mu => mu.idUtente == idUtente);

            if (mangaUtente == null)
            {
                mangaUtente = new MangaUtente
                {
                    idUtente = idUtente,
                    letti = nuoviManga.letti,
                    completati = nuoviManga.completati,
                    preferiti = nuoviManga.preferiti
                };
                _context.MangaUtenti.Add(mangaUtente);
            }
            else
            {
                mangaUtente.letti = nuoviManga.letti ?? mangaUtente.letti;
                mangaUtente.completati = nuoviManga.completati ?? mangaUtente.completati;
                mangaUtente.preferiti = nuoviManga.preferiti ?? mangaUtente.preferiti;

                _context.MangaUtenti.Update(mangaUtente);
            }

            await _context.SaveChangesAsync();
        }
    }
}