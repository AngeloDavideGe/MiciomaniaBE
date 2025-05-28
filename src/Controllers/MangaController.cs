using Miciomania.Data;
using Miciomania.Models.Manga;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Controllers.MangaController
{
    [ApiController]
    [Route("api/manga_miciomania")]
    public class MangaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public MangaController(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        [HttpGet("lista_manga")]
        public async Task<List<Manga>> GetManga()
        {
            string cacheKey = "ListaManga";

            if (_cache.TryGetValue<List<Manga>?>(cacheKey, out var cachedList) && cachedList != null)
            {
                return cachedList;
            }
            else
            {
                List<Manga> mangaList = await _context.Manga.ToListAsync();
                _cache.Set(cacheKey, mangaList, TimeSpan.FromMinutes(30));

                return mangaList;
            }
        }
    }
}
