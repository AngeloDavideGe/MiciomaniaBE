using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data.ApplicationDbContext;
using UserViews;
using MangaMiciomaniaModels;
using MangaUtenteParModels;
using MangaViews;
using CanzoniUtenteModels;
using CanzoniMiciomaniaModels;

namespace Utenti.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParodieController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public ParodieController(AppDbContext context, IDbContextFactory<AppDbContext> contextFactory)
        {
            _context = context;
            _contextFactory = contextFactory;
        }

        [HttpGet("get_all_manga_parodia")]
        public async Task<ActionResult<IEnumerable<UserParams>>> GetAllMangaParodia()
        {
            try
            {
                AllaMangaParodie allManga;

                using (AppDbContext newContext = _contextFactory.CreateDbContext())
                {
                    Task<List<MangaMiciomania>> mangaMicioTask = _context.MangaMicio.ToListAsync();
                    Task<List<MangaUtentePar>> mangaUtenteTask = newContext.MangaUserPar.ToListAsync();

                    await Task.WhenAll(mangaMicioTask, mangaUtenteTask);

                    allManga = new AllaMangaParodie(mangaMicioTask.Result, mangaUtenteTask.Result);
                }

                return Ok(allManga);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpGet("get_all_canzoni_parodia")]
        public async Task<ActionResult<IEnumerable<UserParams>>> GetAllCanzoniParodia()
        {
            try
            {
                AllaCanzoniParodie allCanzoni;

                using (AppDbContext newContext = _contextFactory.CreateDbContext())
                {
                    Task<List<CanzoniMiciomania>> canzoniMicioTask = _context.CanzoniMicio.ToListAsync();
                    Task<List<CanzoniUtente>> canzoniUtenteTask = newContext.CanzoniUser.ToListAsync();

                    await Task.WhenAll(canzoniMicioTask, canzoniUtenteTask);

                    allCanzoni = new AllaCanzoniParodie(canzoniMicioTask.Result, canzoniUtenteTask.Result);
                }

                return Ok(allCanzoni);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }
    }
}
