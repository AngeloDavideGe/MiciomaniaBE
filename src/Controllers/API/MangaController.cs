using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data.ApplicationDbContext;
using MangaViews;
using MangaModels;
using MangaUtenteModels;
using MangaForms;

namespace Manga.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MangaController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public MangaController(AppDbContext context, IDbContextFactory<AppDbContext> contextFactory)
        {
            _context = context;
            _contextFactory = contextFactory;
        }

        [HttpGet("get_all_manga")]
        public async Task<ActionResult<MangaUtente>> GetAllManga()
        {
            List<MangaClass> listaManga = await _context.ListaManga.ToListAsync();

            return Ok(listaManga);
        }

        [HttpGet("get_manga_preferiti")]
        public async Task<ActionResult<MangaUtente>> GetMangaPreferiti([FromQuery] string idutente)
        {
            MangaUtenteGet mangaUtente =
                await GetMangaUtente(_context, idutente)
                ?? new MangaUtenteGet("", "", "");

            return Ok(mangaUtente);
        }

        [HttpGet("get_all_manga_e_preferiti")]
        public async Task<ActionResult<MangaEPreferiti>> GetAllMangaEPreferiti([FromQuery] string idutente)
        {
            try
            {
                Task<List<MangaClass>> mangaTask = _context.ListaManga.ToListAsync();
                Task<MangaUtenteGet?> mangaUtenteTask;

                if (!string.IsNullOrEmpty(idutente))
                {
                    using (var newContext = _contextFactory.CreateDbContext())
                    {
                        mangaUtenteTask = GetMangaUtente(newContext, idutente);
                        await Task.WhenAll(mangaTask, mangaUtenteTask);
                    }
                }
                else
                {
                    await mangaTask;
                    mangaUtenteTask = Task.FromResult<MangaUtenteGet?>(null);
                }

                List<MangaClass> mangaClass = mangaTask.Result;
                MangaUtenteGet mangaUtente = mangaUtenteTask.Result ?? new MangaUtenteGet("", "", "");

                MangaEPreferiti mangaEPreferiti = new MangaEPreferiti(mangaClass, mangaUtente);
                return Ok(mangaEPreferiti);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        private Task<MangaUtenteGet?> GetMangaUtente(AppDbContext context, string idutente)
        {
            return context.MangaUtenti
                .Where((MangaUtente m) => m.idutente == idutente)
                .Select((MangaUtente m) => new MangaUtenteGet(m.preferiti, m.letti, m.completati))
                .FirstOrDefaultAsync();
        }

        [HttpPut("upsert_manga_preferiti/{idutente}")]
        public async Task<ActionResult> UpdateMangaPreferiti(string idutente, [FromBody] MangaUtenteForm nuoviManga)
        {
            try
            {
                MangaUtente? mangaUtente = await _context.MangaUtenti
                    .FirstOrDefaultAsync(mu => mu.idutente == idutente);

                if (mangaUtente == null)
                {
                    mangaUtente = new MangaUtente
                    {
                        idutente = idutente,
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

                return Ok($"Manga Utente aggiornati per l'utente {idutente}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }
    }
}
