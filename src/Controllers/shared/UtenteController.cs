using Interfaces.UserInterfaces;
using Miciomania.Data;
using Miciomania.Form.Proposte;
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
