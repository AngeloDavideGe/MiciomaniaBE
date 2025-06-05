using Miciomania.Views.UserView;
using Miciomania.Data.ApplicationDbContext;
using Miciomania.Forms.Proposte;
using Miciomania.Models.Canzoni;
using Miciomania.Models.Manga;
using Miciomania.Models.Proposte;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Controllers.UserController
{
    [ApiController]
    [Route("api/user_miciomania")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("lista_elementi_utente")]
        public async Task<ElementiUser> GetElemUtente([FromQuery] PropostaPersonaleFormModel model)
        {
            var query =
                from p in _context.Proposta
                    .Where(x => x.id_autore == model.IdUtente)
                    .Take(1)
                    .DefaultIfEmpty()
                join c in _context.Canzone
                    .Where(x => x.id_autore == model.IdUtente)
                    .Take(1)
                    .DefaultIfEmpty() on 1 equals 1 into canzoneJoin
                from c in canzoneJoin.DefaultIfEmpty()
                join m in _context.Manga
                    .Where(x => x.id_autore == model.IdUtente)
                    .Take(1)
                    .DefaultIfEmpty() on 1 equals 1 into mangaJoin
                from m in mangaJoin.DefaultIfEmpty()
                select new ElementiUser
                {
                    Proposta = p ?? new Proposta(),
                    Canzone = c ?? new Canzone(),
                    Manga = m ?? new Manga()
                };

            var result = await query.FirstOrDefaultAsync();

            return result ?? new ElementiUser
            {
                Proposta = new Proposta(),
                Canzone = new Canzone(),
                Manga = new Manga()
            };
        }
    }
}
