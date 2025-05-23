using MiciomaniaNamespace.Interface;
using MiciomaniaNamespace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MiciomaniaNamespace.Controllers
{
    [ApiController]
    [Route("api/manga")]
    public class ListaMangaController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly CopyDbContext _contextCopy;

        public ListaMangaController(AppDbContext context, CopyDbContext contextCopy)
        {
            _context = context;
            _contextCopy = contextCopy;
        }

        [HttpGet("get_all_manga/{id_input}")]
        public async Task<ActionResult<AllManga>> GetAllMangaUtente(string id_input)
        {
            Task<List<ListaManga>> mangaListTask = _context.ListaManga.ToListAsync();
            Task<List<MangaUtente>> mangaUtenteTask = _contextCopy.MangaUtente
                .Where(m => m.id_utente == id_input)
                .ToListAsync();

            await Task.WhenAll(mangaListTask, mangaUtenteTask);

            AllManga result = new AllManga
            {
                ListaManga = mangaListTask.Result,
                MangaUtente = mangaUtenteTask.Result
            };

            return Ok(result);
        }

    }
}
