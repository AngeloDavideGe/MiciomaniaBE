using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data.ApplicationDbContext;
using MangaMiciomaniaModels;
using MangaUtenteParModels;
using MangaViews;
using CanzoniUtenteModels;
using CanzoniMiciomaniaModels;
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

        public ParodieController(AppDbContext context, IDbContextFactory<AppDbContext> contextFactory, IMemoryCache cache)
        {
            _context = context;
            _contextFactory = contextFactory;
            _cache = cache;
        }

        [HttpGet("get_all_manga_parodia")]
        public async Task<ActionResult<AllMangaParodie>> GetAllMangaParodia()
        {
            try
            {
                AllMangaParodie? allManga = await _cache.GetOrCreateAsync("AllMangaParodieCache", async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = _cacheDuration;

                    using (AppDbContext newContext = _contextFactory.CreateDbContext())
                    {
                        Task<List<MangaMiciomania>> mangaMicioTask = _context.MangaMicio.ToListAsync();
                        Task<List<MangaUtentePar>> mangaUtenteTask = newContext.MangaUserPar.ToListAsync();

                        await Task.WhenAll(mangaMicioTask, mangaUtenteTask);

                        return new AllMangaParodie(mangaMicioTask.Result, mangaUtenteTask.Result);
                    }
                });

                return Ok(allManga ?? new AllMangaParodie(
                        new List<MangaMiciomania>(),
                        new List<MangaUtentePar>()
                    )
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpGet("get_all_canzoni_parodia")]
        public async Task<ActionResult<AllCanzoniParodie>> GetAllCanzoniParodia()
        {
            try
            {
                AllCanzoniParodie? allCanzoni = await _cache.GetOrCreateAsync("AllCanzoniParodieCache", async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = _cacheDuration;

                    using (AppDbContext newContext = _contextFactory.CreateDbContext())
                    {
                        Task<List<CanzoniMiciomania>> canzoniMicioTask = _context.CanzoniMicio.ToListAsync();
                        Task<List<CanzoniUtente>> canzoniUtenteTask = newContext.CanzoniUser.ToListAsync();

                        await Task.WhenAll(canzoniMicioTask, canzoniUtenteTask);

                        return new AllCanzoniParodie(canzoniMicioTask.Result, canzoniUtenteTask.Result);
                    }
                });


                return Ok(allCanzoni ?? new AllCanzoniParodie(
                        new List<CanzoniMiciomania>(),
                        new List<CanzoniUtente>()
                    )
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpGet("get_manga_e_canzone_utente/{idUtente}")]
        public async Task<ActionResult<MangaECanzoneUtente>> GetMangaECanzoneUtente(string idUtente)
        {
            try
            {
                MangaECanzoneUtente? mangaECanzoneUtente = await _context.MangaUserPar
                    .Where((MangaUtentePar m) => m.idUtente == idUtente)
                    .Join(_context.CanzoniUser,
                        (MangaUtentePar manga) => manga.idUtente,
                        (CanzoniUtente canzone) => canzone.idUtente,
                        (MangaUtentePar manga, CanzoniUtente canzone) => new MangaECanzoneUtente(manga, canzone))
                    .FirstOrDefaultAsync();

                if (mangaECanzoneUtente == null)
                {
                    return NotFound($"Nessuna parodia trovata per l'utente con ID {idUtente}.");
                }

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
    }
}
