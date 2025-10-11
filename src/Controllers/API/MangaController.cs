using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data.ApplicationDbContext;
using MangaViews;
using MangaModels;
using MangaUtenteModels;
using System.Threading.Tasks;


namespace Manga.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MangaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MangaController(AppDbContext context)
        {
            _context = context;
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
            MangaUtenteGet mangaUtente = await GetMangaUtente(idutente) ?? new MangaUtenteGet
            {
                letti = "",
                completati = "",
                preferiti = "",
            };

            return Ok(mangaUtente);
        }

        [HttpGet("get_all_manga_e_preferiti")]
        public async Task<ActionResult<MangaEPreferiti>> GetAllMangaEPreferiti([FromQuery] string idutente)
        {
            MangaUtenteGet mangaUtente = new MangaUtenteGet { letti = " ", preferiti = " ", completati = " " };
            List<MangaClass> mangaClass = await _context.ListaManga.ToListAsync();

            if (!string.IsNullOrEmpty(idutente))
            {
                mangaUtente = await GetMangaUtente(idutente) ?? mangaUtente;
            }

            MangaEPreferiti mangaEPreferiti = new MangaEPreferiti(mangaClass, mangaUtente);
            return Ok(mangaEPreferiti);
        }

        private Task<MangaUtenteGet?> GetMangaUtente(string idutente)
        {
            return _context.MangaUtenti
                .Where(m => m.idutente == idutente)
                .Select(m => new MangaUtenteGet
                {
                    letti = m.letti,
                    preferiti = m.preferiti,
                    completati = m.completati
                })
                .FirstOrDefaultAsync();
        }
    }
}
