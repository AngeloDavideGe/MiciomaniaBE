using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data.ApplicationDbContext;
using MangaUtenteParModels;
using MangaViews;
using CanzoniUtenteModels;
using ParodieForms;
using Microsoft.Extensions.Caching.Memory;

namespace Utenti.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParodieController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(24);
        private readonly string _mangaCacheKey = "AllMangaParodieCache";
        private readonly string _canzoniCacheKey = "AllCanzoniParodieCache";

        public ParodieController(AppDbContext context, IDbContextFactory<AppDbContext> contextFactory, IMemoryCache cache)
        {
            _context = context;
            _contextFactory = contextFactory;
            _cache = cache;
        }

        [HttpGet("get_all_manga_parodia")]
        public async Task<ActionResult<AllMangaParodie>> GetAllMangaParodia()
        {
            using (AppDbContext newContext = _contextFactory.CreateDbContext())
            {
                return await GetMangaCanzoneCustom(
                    _mangaCacheKey,
                    () => _context.MangaMicio.ToListAsync(),
                    () => newContext.MangaUserPar.ToListAsync(),
                    (mangaMicio, mangaUtente) => new AllMangaParodie(mangaMicio, mangaUtente)
                );
            }
        }

        [HttpGet("get_all_canzoni_parodia")]
        public async Task<ActionResult<AllCanzoniParodie>> GetAllCanzoniParodia()
        {
            using (AppDbContext newContext = _contextFactory.CreateDbContext())
            {
                return await GetMangaCanzoneCustom(
                    _canzoniCacheKey,
                    () => _context.CanzoniMicio.ToListAsync(),
                    () => newContext.CanzoniUser.ToListAsync(),
                    (canzoniMicio, canzoniUtente) => new AllCanzoniParodie(canzoniMicio, canzoniUtente)
                );
            }
        }

        [HttpGet("get_manga_e_canzone_utente/{idUtente}")]
        public async Task<ActionResult<MangaECanzoneUtente>> GetMangaECanzoneUtente(string idUtente)
        {
            try
            {
                Task<MangaUtentePar?> mangaUtenteParTask;
                Task<CanzoniUtente?> canzoniUtenteTask;

                using (AppDbContext newContext = _contextFactory.CreateDbContext())
                {
                    mangaUtenteParTask = _context.MangaUserPar
                       .Where((MangaUtentePar m) => m.idUtente == idUtente)
                       .FirstOrDefaultAsync();

                    canzoniUtenteTask = newContext.CanzoniUser
                        .Where((CanzoniUtente c) => c.idUtente == idUtente)
                        .FirstOrDefaultAsync();

                    await Task.WhenAll(mangaUtenteParTask, canzoniUtenteTask);
                }

                MangaECanzoneUtente mangaECanzoneUtente = new MangaECanzoneUtente(
                    mangaUtenteParTask.Result ?? new MangaUtentePar(),
                    canzoniUtenteTask.Result ?? new CanzoniUtente()
                );

                return Ok(mangaECanzoneUtente);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpPut("upsert_manga_o_canzone/{id}")]
        public async Task<ActionResult> UpdateUser(string id, [FromBody] ParodieUtenteForm parodieForm)
        {
            try
            {
                await _context.Database.ExecuteSqlInterpolatedAsync(
                    $@"SELECT parodie_schema.insert_manga_o_canzone(
                        {id}, 
                        {parodieForm.nome}, 
                        {parodieForm.genere}, 
                        {parodieForm.copertina}, 
                        {parodieForm.url}, 
                        {parodieForm.tipo}
                    )"
                );

                return Ok(new { message = "Parodia aggiornata con successo" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        private async Task<ActionResult<H>> GetMangaCanzoneCustom<T, F, H>(
            string cacheKey,
            Func<Task<T>> taskTFactory,
            Func<Task<F>> taskFFactory,
            Func<T, F, H> resultCreator
        ) where T : class, new() where F : class, new() where H : class
        {
            try
            {
                H? result = await _cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = _cacheDuration;

                    Task<T> task1 = taskTFactory();
                    Task<F> task2 = taskFFactory();

                    await Task.WhenAll(task1, task2);

                    return resultCreator(task1.Result, task2.Result);
                });

                return Ok(result ?? resultCreator(new T(), new F()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }
    }
}
