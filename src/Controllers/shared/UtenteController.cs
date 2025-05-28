using Interfaces.UserInterfaces;
using Miciomania.Data;
using Miciomania.Form.Proposte;
using Miciomania.Models.Manga;
using Miciomania.Models.Proposte;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;  // <-- import per IMemoryCache

namespace Controllers.UserController
{
    [ApiController]
    [Route("api/user_miciomania")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public UserController(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        [HttpGet("lista_elementi_utente")]
        public async Task<ElementiUser> GetElemUtente([FromQuery] PropostaPersonaleFormModel model)
        {
            string cacheKey = $"ElementiUser";

            if (_cache.TryGetValue<ElementiUser?>(cacheKey, out var cachedData) && cachedData != null)
            {
                return cachedData;
            }
            else
            {
                ElementiUser result = await GetElemUtenteFromDb(model);
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(30));

                return result;
            }
        }

        private async Task<ElementiUser> GetElemUtenteFromDb(PropostaPersonaleFormModel model)
        {
            var query = _context.Proposte
                .Where(p => p.id_utente == model.IdUtente)
                .Join(_context.Manga,
                    p => p.id_utente,
                    m => m.id_autore,
                    (p, m) => new ElementiUser
                    {
                        Proposta = p,
                        Manga = m
                    });

            ElementiUser? result = await query.FirstOrDefaultAsync();

            if (result == null)
            {
                return new ElementiUser
                {
                    Proposta = new Proposte(),
                    Manga = new Manga()
                };
            }

            return result;
        }
    }
}
