using Miciomania.Data.ApplicationDbContext;
using Miciomania.Models.Manga;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Controllers.MangaController
{
    [ApiController]
    [Route("api/manga_miciomania")]
    public class MangaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MangaController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("lista_manga")]
        public async Task<List<Manga>> GetManga()
        {
            return await _context.Manga.ToListAsync();
        }
    }
}
