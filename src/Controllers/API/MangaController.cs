using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data.ApplicationDbContext;
using MangaViews;
using MangaModels;
using MangaUtenteModels;
using MangaForms;
using Microsoft.Extensions.Caching.Memory;

namespace Manga.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MangaController : UtilitiesController
    {
        public MangaController(
            AppDbContext context,
            IDbContextFactory<AppDbContext> contextFactory,
            IMemoryCache cache
        ) : base(context, contextFactory, cache) { }

        [HttpGet("get_all_manga")]
        public async Task<ActionResult<List<MangaClass>>> GetAllManga()
        {
            return await SingleTask(new SingleTaskOptions<List<MangaClass>>
            {
                Task = GetAllMangaCache,
                ErrorMessage = "Errore nel recupero dei manga"
            });
        }

        [HttpGet("get_manga_preferiti")]
        public async Task<ActionResult<MangaUtente>> GetMangaPreferiti([FromQuery] string idUtente)
        {
            MangaUtenteGet mangaUtente =
                await GetMangaUtente(_context, idUtente)
                ?? new MangaUtenteGet("", "", "");

            return Ok(mangaUtente);
        }

        [HttpGet("get_all_manga_e_preferiti")]
        public async Task<ActionResult<MangaEPreferiti>> GetAllMangaEPreferiti([FromQuery] string idUtente)
        {
            return await MultiTask(new MultiTaskOptions<List<MangaClass>, MangaUtenteGet?, MangaEPreferiti>
            {
                Task1 = GetAllMangaCache,

                Task2 = () => string.IsNullOrEmpty(idUtente)
                    ? Task.FromResult<MangaUtenteGet?>(null)
                    : GetMangaUtente(_contextFactory.CreateDbContext(), idUtente),

                ResultFactory = (manga, preferiti) =>
                    new MangaEPreferiti(
                        manga,
                        preferiti ?? new MangaUtenteGet("", "", "")
                    ),

                ErrorMessage = "Errore nel recupero Manga e Preferiti"
            });
        }

        [HttpPost("post_manga")]
        public async Task<ActionResult> PostManga([FromBody] MangaClass mangaForm)
        {
            return await SqlFunc(new SqlTaskOptions
            {
                Sql = async () =>
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
                    await _context.SaveChangesAsync();
                },

                SuccessMessage = "Manga aggiunto con successo",
                ErrorMessage = "Errore inserimento manga"
            });
        }

        [HttpPut("upsert_manga_preferiti/{idUtente}")]
        public async Task<ActionResult> UpdateMangaPreferiti(string idUtente, [FromBody] MangaUtenteForm nuoviManga)
        {
            return await SqlFunc(new SqlTaskOptions
            {
                Sql = async () =>
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
                },
                SuccessMessage = $"Manga utente aggiornati per {idUtente}",
                ErrorMessage = "Errore aggiornamento manga utente"
            });
        }

        // Metodi di supporto
        private Task<MangaUtenteGet?> GetMangaUtente(AppDbContext context, string idUtente)
        {
            return context.MangaUtenti
                .Where((MangaUtente m) => m.idUtente == idUtente)
                .Select((MangaUtente m) => new MangaUtenteGet(m.preferiti, m.letti, m.completati))
                .FirstOrDefaultAsync();
        }

        private async Task<List<MangaClass>> GetAllMangaCache()
        {
            return await CacheFunc(new CacheOptions<List<MangaClass>>
            {
                Task = () => _context.ListaManga.ToListAsync(),
                NomeCache = "AllManga",
                DurataCache = TimeSpan.FromHours(2)
            });
        }
    }
}
