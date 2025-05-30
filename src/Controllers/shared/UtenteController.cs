using Miciomania.Views.UserView;
using Miciomania.Data;
using Miciomania.Form.Proposte;
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
            var query = _context.Proposta
                .Where(p => p.id_utente == model.IdUtente)
                .Join(_context.Canzone,
                    p => p.id_utente,
                    c => c.id_autore,
                    (p, c) => new { Proposta = p, Canzone = c })
                .Join(_context.Manga,
                    pc => pc.Proposta.id_utente,
                    m => m.id_autore,
                    (pc, m) => new ElementiUser
                    {
                        Proposta = pc.Proposta,
                        Canzone = pc.Canzone,
                        Manga = m
                    });

            ElementiUser? result = await query.FirstOrDefaultAsync();

            if (result == null)
            {
                return new ElementiUser
                {
                    Canzone = new Canzone(),
                    Proposta = new Proposta(),
                    Manga = new Manga(),
                };
            }

            return result;
        }
    }
}
