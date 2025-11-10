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
    public class MangaController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(24);
        private readonly string _mangaCacheKey = "AllMangaCache";

        public MangaController(AppDbContext context, IDbContextFactory<AppDbContext> contextFactory, IMemoryCache cache)
        {
            _context = context;
            _contextFactory = contextFactory;
            _cache = cache;
        }

        [HttpGet("get_all_manga")]
        public async Task<ActionResult<List<MangaClass>>> GetAllManga()
        {
            List<MangaClass> listaManga = await GetAllMangaCache(_context);
            return Ok(listaManga);
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
            try
            {
                Task<List<MangaClass>> mangaTask;
                Task<MangaUtenteGet?> mangaUtenteTask;

                if (!string.IsNullOrEmpty(idUtente))
                {
                    using (AppDbContext newContext = _contextFactory.CreateDbContext())
                    {
                        mangaTask = GetAllMangaCache(_context);
                        mangaUtenteTask = GetMangaUtente(newContext, idUtente);

                        await Task.WhenAll(mangaTask, mangaUtenteTask);
                    }
                }
                else
                {
                    mangaTask = GetAllMangaCache(_context);
                    mangaUtenteTask = Task.FromResult<MangaUtenteGet?>(null);

                    await mangaTask;
                }

                MangaEPreferiti mangaEPreferiti = new MangaEPreferiti(
                    mangaTask.Result,
                    mangaUtenteTask.Result ?? new MangaUtenteGet("", "", "")
                );

                return Ok(mangaEPreferiti);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpPost("post_manga")]
        public async Task<ActionResult> PostTweet([FromBody] MangaClass mangaForm)
        {
            try
            {
                MangaClass newManga = new MangaClass
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

                List<MangaClass>? cachedManga = _cache.Get<List<MangaClass>>(_mangaCacheKey);
                if (cachedManga != null)
                {
                    cachedManga.Add(newManga);
                    _cache.Set(_mangaCacheKey, cachedManga, _cacheDuration);
                }

                return Ok("Tweet aggiunto con successo");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex}");
            }
        }

        [HttpPut("upsert_manga_preferiti/{idUtente}")]
        public async Task<ActionResult> UpdateMangaPreferiti(string idUtente, [FromBody] MangaUtenteForm nuoviManga)
        {
            try
            {
                MangaUtente? mangaUtente = await _context.MangaUtenti
                    .FirstOrDefaultAsync((MangaUtente mu) => mu.idUtente == idUtente);

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

                return Ok($"Manga Utente aggiornati per l'utente {idUtente}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        // Metodi di supporto
        private Task<MangaUtenteGet?> GetMangaUtente(AppDbContext context, string idUtente)
        {
            return context.MangaUtenti
                .Where((MangaUtente m) => m.idUtente == idUtente)
                .Select((MangaUtente m) => new MangaUtenteGet(m.preferiti, m.letti, m.completati))
                .FirstOrDefaultAsync();
        }

        private async Task<List<MangaClass>> GetAllMangaCache(AppDbContext context)
        {
            List<MangaClass>? listaManga = await _cache.GetOrCreateAsync(_mangaCacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _cacheDuration;
                entry.SetPriority(CacheItemPriority.Normal);
                return await context.ListaManga.ToListAsync();
            });

            return listaManga ?? new List<MangaClass>();
        }
    }
}
